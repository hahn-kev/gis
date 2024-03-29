﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Entities.Helper;
using Backend.Utils;
using LinqToDB;

namespace Backend.Services
{
    public class LeaveService
    {
        private readonly OrgGroupRepository _orgGroupRepository;

        private readonly PersonRepository _personRepository;
        private readonly LeaveRequestRepository _leaveRequestRepository;
        private readonly IEntityService _entityService;
        private readonly LeaveRequestEmailService _leaveRequestEmailService;

        public LeaveService(OrgGroupRepository orgGroupRepository,
            PersonRepository personRepository,
            LeaveRequestRepository leaveRequestRepository,
            IEntityService entityService,
            LeaveRequestEmailService leaveRequestEmailService)
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _entityService = entityService;
            _leaveRequestEmailService = leaveRequestEmailService;
        }

        public IList<LeaveRequestWithNames> LeaveRequestsWithNames =>
            _leaveRequestRepository.LeaveRequestWithNames.ToList();

        public List<LeaveRequestPublic> PublicLeaveRequests()
        {
            return _leaveRequestRepository.PublicLeaveRequests();
        }

        private IList<LeaveRequestWithNames> ListForPeople(IQueryable<Guid> people)
        {
            return (from request in _leaveRequestRepository.LeaveRequestWithNames
                from personId in people.InnerJoin(personId => personId == request.PersonId)
                select request).ToList();
        }

        public IList<LeaveRequestWithNames> ListByPersonId(Guid personId)
        {
            return _leaveRequestRepository.LeaveRequestWithNames
                .Where(request => request.PersonId == personId)
                .ToList();
        }

        public IList<LeaveRequestWithNames> ListUnderOrgGroup(Guid orgGroupId, Guid? includePersonId = null)
        {
            return ListForPeople(
                PeopleWithStaffUnderGroup(orgGroupId)
//                        .Union(_personRepository.PeopleWithStaff.Where(p => p.Id == includePersonId))
                    .Select(person => person.Id)
            );
        }

        public IList<LeaveRequestWithNames> ListForSupervisor(Guid supervisorId)
        {
            return (from request in _leaveRequestRepository.LeaveRequestWithNames
                from person in _personRepository.PeopleExtended.InnerJoin(person => person.Id == request.PersonId)
                from staff in _personRepository.Staff.InnerJoin(staff => staff.Id == person.StaffId)
                from orgGroup in _orgGroupRepository.GetBySupervisorIdWithChildren(supervisorId)
                    .InnerJoin(orgGroup => orgGroup.Id == staff.OrgGroupId)
                select request).ToList();
        }

        public LeaveRequestWithNames GetById(Guid id)
        {
            return _leaveRequestRepository.LeaveRequestWithNames.SingleOrDefault(request => request.Id == id) ??
                   throw new UserError("Unable to find leave request " + id + " it may have been deleted");
        }

        public Guid? GetLeavePersonId(Guid leaveId) =>
            _leaveRequestRepository.LeaveRequests.Where(request => request.Id == leaveId)
                .Select(request => (Guid?) request.PersonId).SingleOrDefault();

        public void UpdateLeave(LeaveRequest leaveRequest)
        {
            //validation done in ThrowIfHrRequiredForUpdate
            _entityService.Save(leaveRequest);
        }

        public void DeleteLeaveRequest(Guid id)
        {
            _entityService.Delete<LeaveRequest>(id);
        }

        public Task<(bool updated, PersonWithStaff requester, bool notified)> ApproveLeaveRequest(
            Guid leaveRequestId,
            Guid supervisorId)
        {
            var leaveRequest =
                _leaveRequestRepository.LeaveRequests.SingleOrDefault(request => request.Id == leaveRequestId);
            if (leaveRequest == null)
            {
                throw new UserError("Unable to find Leave request matching Id: " + leaveRequestId +
                                    " it may have been deleted");
            }

            return ApproveLeaveRequest(leaveRequest, supervisorId);
        }

        public async Task<(bool updated, PersonWithStaff requester, bool notified)> ApproveLeaveRequest(
            LeaveRequest leaveRequest,
            Guid supervisorId)
        {
            if (leaveRequest == null)
                throw new ArgumentNullException(nameof(leaveRequest), "Leave request may have been deleted");
            var people = _personRepository.FindByIds(new[] {supervisorId, leaveRequest.PersonId});
            if (!people.TryGetValue(supervisorId, out var supervisor))
            {
                throw new UserError("Unable to find supervisor matching Id: " + supervisorId +
                                    " they may have been deleted");
            }

            if (!people.TryGetValue(leaveRequest.PersonId, out var requester))
            {
                throw new UserError("Unable to find leave requester matching Id: " + leaveRequest.PersonId +
                                    " they may have been deleted");
            }

            var updated = _leaveRequestRepository.ApproveLeaveRequest(leaveRequest.Id, supervisor.Id);
            var notified = false;
            if (!string.IsNullOrEmpty(requester.Staff?.Email))
            {
                await _leaveRequestEmailService.NotifyRequestApproved(leaveRequest, requester, supervisor);
                notified = true;
            }

            return (updated, requester, notified);
        }

        public async Task<Person> RequestLeave(LeaveRequest leaveRequest)
        {
            if (leaveRequest == null) throw new ArgumentNullException(nameof(leaveRequest));
            var personOnLeave = _personRepository.PeopleWithStaff.SingleOrDefault(p => p.Id == leaveRequest.PersonId);
            if (personOnLeave?.StaffId == null)
                throw new UnauthorizedAccessException("Person requesting leave must be staff");
            leaveRequest.Approved = null;
            leaveRequest.ApprovedById = null;
            var leaveUsage = GetLeaveUseage(leaveRequest.Type, personOnLeave.Id, leaveRequest.StartDate.SchoolYear());
            var isNew = leaveRequest.IsNew();
            _entityService.Save(leaveRequest);
            try
            {
                var (toApprove, toNotify) = ResolveLeaveRequestEmails(personOnLeave);
                await SendLeaveRequestEmails(leaveRequest, personOnLeave, toApprove, toNotify, leaveUsage);
                return toApprove;
            }
            catch
            {
                if (isNew)
                    _entityService.Delete(leaveRequest);
                throw;
            }
        }

        public async Task SendLeaveRequestEmails(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff toApprove,
            IEnumerable<PersonWithStaff> toNotify,
            LeaveUsage leaveUsage)
        {
            if (toApprove == null)
                throw new UserError(
                    $"Unable to find Supervisor for: {requestedBy.PreferredName} {requestedBy.LastName}");
            await _leaveRequestEmailService.SendRequestApproval(leaveRequest,
                requestedBy,
                toApprove,
                leaveUsage);
            await Task.WhenAll(toNotify.Where(person => person.Id != toApprove?.Id).Select(supervisor =>
                _leaveRequestEmailService.NotifyOfLeaveRequest(leaveRequest,
                    requestedBy,
                    supervisor,
                    toApprove,
                    leaveUsage)));

            await _leaveRequestEmailService.NotifyHr(leaveRequest, requestedBy, toApprove, leaveUsage);
        }

        public (PersonWithStaff toApprove, List<PersonWithStaff> toNotify) ResolveLeaveRequestEmails(
            PersonWithStaff requestedBy)
        {
            return ResolveLeaveRequestEmails(requestedBy,
                _orgGroupRepository.StaffParentOrgGroups(requestedBy.Staff),
                _orgGroupRepository.GetOrgGroupsByPersonsRole(requestedBy.Id));
        }

        public static (PersonWithStaff toApprove, List<PersonWithStaff> toNotify) ResolveLeaveRequestEmails(
            PersonWithStaff requestedBy,
            IEnumerable<OrgGroupWithSupervisor> approvalGroups,
            IEnumerable<OrgGroupWithSupervisor> roleGroups)
        {
            var supervisorsToNotify = new List<PersonWithStaff>(roleGroups.Select(org => org.SupervisorPerson));
            PersonWithStaff toApprove = null;
            foreach (var orgGroup in OrgGroupService.SortOrgGroupByHierarchy(approvalGroups,
                OrgGroupService.SortedBy.ChildFirst))
            {
                //super and requested by will be the same if the requester is a supervisor
                if (orgGroup == null || requestedBy.Id == orgGroup.Supervisor ||
                    orgGroup.SupervisorPerson == null) continue;
                if (orgGroup.ApproverIsSupervisor)
                {
                    toApprove = orgGroup.SupervisorPerson;
                    break;
                }

                supervisorsToNotify.Add(orgGroup.SupervisorPerson);
            }

            return (toApprove,
                supervisorsToNotify
                    .Where(toNotify => toNotify.Id != toApprove?.Id && toNotify.Id != requestedBy.Id)
                    .DistinctBy(staff => staff.Id)
                    .ToList());
        }

        public LeaveDetails GetCurrentLeaveDetails(PersonWithOthers person)
        {
            return GetLeaveDetails(person.Id, person.Roles, DateTime.Now.SchoolYear());
        }

        public LeaveDetails GetCurrentLeaveDetails(Guid personId)
        {
            return GetLeaveDetails(personId, DateTime.Now.SchoolYear());
        }

        public LeaveDetails GetLeaveDetails(Guid personId, int schoolYear)
        {
            return GetLeaveDetails(personId,
                _personRepository.GetPersonRolesWithJob(personId),
                schoolYear);
        }

        public LeaveDetails GetLeaveDetails(Guid personId, IEnumerable<PersonRoleWithJob> personRoles, int schoolYear)
        {
            var leaveRequests = _personRepository.LeaveRequestsInYear(schoolYear)
                .Where(request => request.PersonId == personId).ToList();
            return new LeaveDetails
            {
                LeaveUsages = CalculateLeaveDetails(personRoles, leaveRequests, schoolYear)
            };
        }

        private List<LeaveUsage> CalculateLeaveDetails(IEnumerable<PersonRoleWithJob> personRoles,
            IEnumerable<LeaveRequest> leaveRequests,
            int schoolYear)
        {
            var leaveTypes = Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>();
            var vacationAllowed = LeaveAllowed(LeaveType.Vacation, personRoles, schoolYear);
            //we do this whole group join thing so that we get a result for each leave type
            //even if there's no requests for them
            return leaveTypes.GroupJoin(leaveRequests,
                type => type,
                request => request.Type,
                (type, requests) => new LeaveUsage
                {
                    LeaveType = type,
                    Used = TotalLeaveUsed(requests),
                    TotalAllowed = type == LeaveType.Vacation
                        ? vacationAllowed
                        : LeaveAllowed(type, Enumerable.Empty<PersonRoleWithJob>(), schoolYear)
                }
            ).ToList();
        }

        public LeaveUsage GetCurrentLeaveUseage(LeaveType leaveType, Guid personId)
        {
            return GetLeaveUseage(leaveType, personId, DateTime.Now.SchoolYear());
        }

        public LeaveUsage GetLeaveUseage(LeaveType leaveType, Guid personId, int schoolYear)
        {
            return new LeaveUsage
            {
                LeaveType = leaveType,
                TotalAllowed = LeaveAllowed(leaveType, personId),
                Used = TotalLeaveUsed(leaveType, personId, schoolYear)
            };
        }

        public static decimal TotalLeaveUsed(IEnumerable<LeaveRequest> requests)
        {
            //filter out rejected requests, but pending (null) and approved should be counted
            return requests.Where(request => request.Approved != false).Sum(request => request.Days);
        }

        public decimal TotalLeaveUsed(LeaveType leaveType, Guid personId, int schoolYear)
        {
            return TotalLeaveUsed(_personRepository.LeaveRequests
                .Where(request => request.PersonId == personId
                                  && request.StartDate.InSchoolYear(schoolYear)
                                  && request.Type == leaveType).ToList());
        }

        public int LeaveAllowed(LeaveType leaveType, Guid personId)
        {
            return LeaveAllowed(leaveType, personId, DateTime.Now.SchoolYear());
        }

        public int LeaveAllowed(LeaveType leaveType, Guid personId, int schoolYear)
        {
            if (leaveType != LeaveType.Vacation)
                return LeaveAllowed(leaveType, Enumerable.Empty<PersonRoleWithJob>(), schoolYear);
            //fetch personRoles
            return LeaveAllowed(leaveType, _personRepository.GetPersonRolesWithJob(personId), schoolYear);
        }

        public static int LeaveAllowed(LeaveType leaveType, IEnumerable<PersonRoleWithJob> personRoles, int schoolYear)
        {
            switch (leaveType)
            {
                case LeaveType.Sick: return 30;
                case LeaveType.Personal: return 5;
                case LeaveType.Emergency: return 5;
                case LeaveType.Maternity: return 90;
                case LeaveType.Paternity: return 5;
                case LeaveType.Vacation: break;
                default: return 0;
            }

            //calculation for vacation time is done here
            var totalServiceTime = TimeSpan.Zero;
            var jobStatusWithLeave = new[] {JobStatus.FullTime, JobStatus.HalfTime, JobStatus.FullTime10Mo};
            var validRoles = new List<PersonRoleWithJob>();
            foreach (var role in personRoles)
            {
                if (role.Job.OrgGroup?.Type != GroupType.Department && role.Job.OrgGroup?.Supervisor == role.PersonId &&
                    role.Active) return 20;
                // start date school year check is to only include roles that were valid during the year being queried
                if (role.Job.Status.HasValue && jobStatusWithLeave.Contains(role.Job.Status.Value) && role.StartDate.SchoolYear() <= schoolYear)
                    validRoles.Add(role);
            }

            if (validRoles.Where(r => r.ActiveDuringYear(schoolYear)).All(r => r.Job.Status == JobStatus.FullTime10Mo))
                return 0;

            totalServiceTime = ServiceLength(validRoles, schoolYear);
            //no time has been spent as staff or a director, therefore no vacation time is allowed
            if (totalServiceTime == TimeSpan.Zero) return 0;
            //todo pick cut off and days to count out of
            if (totalServiceTime.Days < 300) return (int) Math.Truncate(totalServiceTime.Days / 365m * 10);
            var yearsOfService = totalServiceTime.Days / 365;
            if (yearsOfService < 10) return 10;
            if (yearsOfService < 20) return 15;
            return 20;
        }

        public static TimeSpan ServiceLength(IEnumerable<PersonRole> validRoles, int schoolYear)
        {
            var totalServiceTime = TimeSpan.Zero;
            var ranges = validRoles.Select(role =>
            {
                var startDate = role.StartDate;
                var endDate = role.Active
                    ? schoolYear.EndOfSchoolYear()
                    : role.EndDate ?? throw new ArgumentException("unknown end date for role:" + role.Id);
                return new DateRange(startDate, endDate);
            });
            ranges = DateRange.Combine(ranges);

            foreach (var dateRange in ranges)
            {
                totalServiceTime += dateRange.Length;
            }

            return totalServiceTime;
        }

        public PersonAndLeaveDetails PersonWithCurrentLeave(Guid personId)
        {
            return PersonWithLeave(personId, DateTime.Now.SchoolYear());
        }

        public PersonAndLeaveDetails PersonWithLeave(Guid personId, int schoolYear)
        {
            var peopleQueryable = _personRepository.PeopleWithStaff.Where(person =>
                person.StaffId != null && person.Id == personId);
            return PeopleWithLeave(peopleQueryable, schoolYear).FirstOrDefault();
        }

        public IList<PersonAndLeaveDetails> PeopleInGroupWithLeave(Guid orgGroupId, int schoolYear)
        {
            var peopleWithStaff = PeopleWithStaffUnderGroup(orgGroupId);
            return PeopleWithLeave(peopleWithStaff, schoolYear);
        }

        public IList<PersonAndLeaveDetails> PeopleWithCurrentLeave()
        {
            return PeopleWithLeave(DateTime.Now.SchoolYear());
        }

        public IList<PersonAndLeaveDetails> PeopleWithLeave(int schoolYear)
        {
            return PeopleWithLeave(_personRepository.PeopleWithStaff.Where(staff => staff.StaffId != null)
                    .OrderBy(_ => _.PreferredName ?? _.FirstName).ThenBy(_ => _.LastName),
                schoolYear);
        }

        private IList<PersonAndLeaveDetails> PeopleWithLeave(IQueryable<PersonWithStaff> peopleQueryable,
            int schoolYear)
        {
            var leaveRequests = (
                    from request in _personRepository.LeaveRequestsInYear(schoolYear)
                    from p in peopleQueryable.InnerJoin(person => person.Id == request.PersonId)
                    select request)
                .ToLookup(request => request.PersonId);
            var personRoles = (
                    from role in _personRepository.PersonRolesWithJob
                    from person in peopleQueryable.InnerJoin(person => person.Id == role.PersonId)
                    select role)
                .ToLookup(role => role.PersonId);

            return peopleQueryable.Select(person => new PersonAndLeaveDetails
            {
                Person = person,
                LeaveUsages = CalculateLeaveDetails(personRoles[person.Id], leaveRequests[person.Id], schoolYear)
            }).ToList();
        }

        public void ThrowIfHrRequiredForUpdate(LeaveRequest updatedLeaveRequest)
        {
            LeaveRequest oldRequest = null;
            if (!updatedLeaveRequest.IsNew())
                oldRequest = _leaveRequestRepository.LeaveRequests.SingleOrDefault(request =>
                    request.Id == updatedLeaveRequest.Id);
            ThrowIfHrRequiredForUpdate(oldRequest, updatedLeaveRequest, _entityService.List<Holiday>());
        }

        public IQueryable<PersonWithStaff> PeopleWithStaffUnderGroup(Guid orgGroupId)
        {
            return (from person in _personRepository.PeopleWithStaff
                from org in _orgGroupRepository.GetByIdWithChildren(orgGroupId)
                    .InnerJoin(orgGroup => orgGroup.Id == person.Staff.OrgGroupId)
                select person).OrderBy(_ => _.PreferredName ?? _.FirstName).ThenBy(_ => _.LastName);
        }

        public static void ThrowIfHrRequiredForUpdate(LeaveRequest oldRequest,
            LeaveRequest newRequest,
            List<Holiday> holidays)
        {
            var newDays = CalculateLeaveDays(newRequest, holidays);
            if (oldRequest == null)
            {
                if (newRequest.OverrideDays)
                {
                    throw new UnauthorizedAccessException(
                        "You're not allowed to override the leave calculation, talk to HR");
                }

                if (newRequest.Days != newDays &&
                    newRequest.Days != newDays - 0.5m)
                {
                    throw new ArgumentException(
                        $"Leave request days calculated didn't match what was expected for dates {newRequest.StartDate} to {newRequest.EndDate}");
                }

                return;
            }

            var oldDays = CalculateLeaveDays(oldRequest, holidays);

            if (oldRequest.Approved != null)
            {
                throw new UnauthorizedAccessException("You're not allowed to modify approved leave requests");
            }

            if (oldRequest.PersonId != newRequest.PersonId)
            {
                throw new UnauthorizedAccessException("You aren't allowed to change the person requesting leave");
            }

            if (!oldRequest.OverrideDays && newRequest.OverrideDays)
            {
                throw new UnauthorizedAccessException(
                    "You aren't allowed to override the length of this leave request, talk to HR");
            }

            if (!newRequest.OverrideDays && newRequest.Days != newDays &&
                newRequest.Days != newDays - 0.5m)
            {
                throw new UnauthorizedAccessException(
                    "Days of leave request must match calculated when not being overriden");
            }

            if (oldRequest.OverrideDays && oldRequest.Days != newRequest.Days)
            {
                throw new UnauthorizedAccessException(
                    "You aren't allowed modify the length of this leave request, talk to HR");
            }

            if (oldRequest.OverrideDays && newRequest.OverrideDays &&
                oldDays != newDays)
            {
                throw new UnauthorizedAccessException(
                    "You aren't allowed modify the start or end dates of this leave request because the calculation has been overriden, talk to HR");
            }

            if (oldRequest.Approved != newRequest.Approved)
            {
                throw new UnauthorizedAccessException(
                    "You aren't allowed to approve your own leave request, talk to HR");
            }
        }

        public static int CalculateLeaveDays(LeaveRequest leaveRequest, List<Holiday> holidays)
        {
            var days = leaveRequest.CalculateLength();
            foreach (var holiday in holidays)
            {
                days -= WeekdaysOverlapping(leaveRequest, holiday);
            }

            return days;
        }

        public static int WeekdaysOverlapping(LeaveRequest leaveRequest, Holiday holiday)
        {
            var holidayEnd = holiday.End.Date;
            var holidayStart = holiday.Start.Date;
            var leaveStart = leaveRequest.StartDate.Date;
            var leaveEnd = leaveRequest.EndDate.Date;
            if (holidayEnd < leaveStart || holidayStart > leaveEnd)
                return 0;

            if (holidayStart == holidayEnd)
                return holidayStart.Between(leaveStart, leaveEnd) ? 1 : 0;
            if (leaveStart == leaveEnd)
                return leaveStart.Between(holidayStart, holidayEnd) ? 1 : 0;

            //key leave = () holiday = []
            //([])
            if (leaveStart <= holidayStart && holidayEnd <= leaveEnd)
                return holidayStart.BusinessDaysUntil(holidayEnd);
            //[()]
            if (holidayStart < leaveStart && leaveEnd < holidayEnd)
                return leaveStart.BusinessDaysUntil(leaveEnd);

            //([)]
            if (leaveStart < holidayStart && leaveEnd < holidayEnd && holidayStart <= leaveEnd)
                return holidayStart.BusinessDaysUntil(leaveEnd);
            //[(])
            if (holidayStart < leaveStart && holidayEnd < leaveEnd && leaveStart <= holidayEnd)
                return leaveStart.BusinessDaysUntil(holidayEnd);

            throw new Exception(
                $"Not sure how holiday: {holidayStart} -> {holidayEnd} overlaps with {leaveStart} -> {leaveEnd}");
        }
    }
}
