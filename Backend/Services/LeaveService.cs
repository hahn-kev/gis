using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Services
{
    public class LeaveService
    {
        private readonly OrgGroupRepository _orgGroupRepository;

        private readonly PersonRepository _personRepository;
        private readonly LeaveRequestRepository _leaveRequestRepository;
        private readonly IEntityService _entityService;
        private readonly IAuthorizationService _authorizationService;
        private readonly LeaveRequestEmailService _leaveRequestEmailService;

        public LeaveService(OrgGroupRepository orgGroupRepository,
            PersonRepository personRepository,
            LeaveRequestRepository leaveRequestRepository,
            IEntityService entityService,
            IAuthorizationService authorizationService,
            LeaveRequestEmailService leaveRequestEmailService)
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _entityService = entityService;
            _authorizationService = authorizationService;
            _leaveRequestEmailService = leaveRequestEmailService;
        }

        public IList<LeaveRequestWithNames> LeaveRequestsWithNames =>
            _leaveRequestRepository.LeaveRequestWithNames.ToList();

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
            return _leaveRequestRepository.LeaveRequestWithNames.Single(request => request.Id == id);
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

        public async Task<(bool updated, PersonWithStaff notified)> ApproveLeaveRequest(Guid leaveRequestId,
            Guid supervisorId)
        {
            //ensure that this id is a valid person before we approve it
            var leaveRequest =
                _leaveRequestRepository.LeaveRequests.SingleOrDefault(request => request.Id == leaveRequestId);
            if (leaveRequest == null) return (false, null);
            var people = _personRepository.FindByIds(new[] {supervisorId, leaveRequest.PersonId});
            if (!people.TryGetValue(supervisorId, out var supervisor))
            {
                return (false, null);
            }

            var requester = people[leaveRequest.PersonId];
            var updated = _leaveRequestRepository.ApproveLeaveRequest(leaveRequestId, supervisor.Id);
            await _leaveRequestEmailService.NotifyRequestApproved(leaveRequest, requester, supervisor);
            return (updated, requester);
        }

        public async Task<Person> RequestLeave(LeaveRequest leaveRequest)
        {
            var personOnLeave = _personRepository.PeopleWithStaff.SingleOrDefault(p => p.Id == leaveRequest.PersonId);
            if (personOnLeave?.StaffId == null)
                throw new UnauthorizedAccessException("Person requesting leave must be staff");
            var result = _orgGroupRepository.StaffParentOrgGroups(personOnLeave.Staff).ToList();
            leaveRequest.Approved = null;
            leaveRequest.ApprovedById = null;
            var leaveUsage = GetLeaveUseage(leaveRequest.Type, personOnLeave.Id, leaveRequest.StartDate.SchoolYear());
            var isNew = leaveRequest.IsNew();
            _entityService.Save(leaveRequest);
            try
            {
                var supervisor = await ResolveLeaveRequestEmails(leaveRequest,
                    personOnLeave,
                    result,
                    leaveUsage);
                await _leaveRequestEmailService.NotifyHr(leaveRequest, personOnLeave, supervisor, leaveUsage);
                return supervisor;
            }
            catch
            {
                if (isNew)
                    _entityService.Delete(leaveRequest);
                throw;
            }
        }

        public async Task<PersonExtended> ResolveLeaveRequestEmails(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            List<OrgGroupWithSupervisor> orgGroups,
            LeaveUsage leaveUsage)
        {
            if (!OrgGroupService.IsOrgGroupSortedByHierarchy(orgGroups, OrgGroupService.SortedBy.ChildFirst))
                throw new ArgumentException("org groups not sorted properly");
            var supervisorsToNotify = new List<PersonWithStaff>();
            PersonWithStaff approver = null;
            foreach (var orgGroup in orgGroups)
            {
                //super and requested by will be the same if the requester is a supervisor
                if (orgGroup == null || requestedBy.Id == orgGroup.Supervisor ||
                    orgGroup.SupervisorPerson == null) continue;
                if (orgGroup.ApproverIsSupervisor)
                {
                    await _leaveRequestEmailService.SendRequestApproval(leaveRequest,
                        requestedBy,
                        orgGroup.SupervisorPerson,
                        leaveUsage);
                    approver = orgGroup.SupervisorPerson;
                    break;
                }

                supervisorsToNotify.Add(orgGroup.SupervisorPerson);
            }

            await Task.WhenAll(supervisorsToNotify.Where(person => person.Id != approver?.Id).Select(supervisor =>
                _leaveRequestEmailService.NotifyOfLeaveRequest(leaveRequest,
                    requestedBy,
                    supervisor,
                    approver,
                    leaveUsage)));

            return approver;
        }


        public async ValueTask<bool> CanRequestLeave(ClaimsPrincipal user, LeaveRequest leaveRequest)
        {
            if (leaveRequest.PersonId == user.PersonId()) return true;
            if ((await _authorizationService.AuthorizeAsync(user, "leaveRequest")).Succeeded) return true;
            var groupId = user.LeaveDelegateGroupId() ?? user.SupervisorGroupId();
            if (groupId != null)
                return PeopleWithStaffUnderGroup(groupId.Value).Any(person => person.Id == leaveRequest.PersonId);
            return false;
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
                LeaveUsages = CalculateLeaveDetails(personRoles, leaveRequests)
            };
        }

        private List<LeaveUsage> CalculateLeaveDetails(IEnumerable<PersonRoleWithJob> personRoles,
            IEnumerable<LeaveRequest> leaveRequests)
        {
            var leaveTypes = Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>();
            //we do this here so we can make personRoles enumerable
            var vacationAllowed = LeaveAllowed(LeaveType.Vacation, personRoles);
            return leaveTypes.GroupJoin(leaveRequests,
                type => type,
                request => request.Type,
                (type, requests) => new LeaveUsage
                {
                    LeaveType = type,
                    Used = TotalLeaveUsed(requests),
                    TotalAllowed = type == LeaveType.Vacation
                        ? vacationAllowed
                        : LeaveAllowed(type, Enumerable.Empty<PersonRoleWithJob>())
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
            if (leaveType != LeaveType.Vacation) return LeaveAllowed(leaveType, Enumerable.Empty<PersonRoleWithJob>());
            //fetch personRoles
            return LeaveAllowed(leaveType, _personRepository.GetPersonRolesWithJob(personId));
        }

        public static int LeaveAllowed(LeaveType leaveType, IEnumerable<PersonRoleWithJob> personRoles)
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
            foreach (var role in personRoles)
            {
                if (role.Job.OrgGroup?.Type != GroupType.Department && role.Job.OrgGroup?.Supervisor == role.PersonId &&
                    role.Active) return 20;
                if (role.Job.Status.HasValue && jobStatusWithLeave.Contains(role.Job.Status.Value))
                    totalServiceTime = totalServiceTime + role.LengthOfService();
            }

            //no time has been spent as staff or a director, therefore no vacation time is allowed
            if (totalServiceTime == TimeSpan.Zero) return 0;
            //todo pick cut off and days to count out of
            if (totalServiceTime.Days < 300) return (int) Math.Truncate(totalServiceTime.Days / 365m * 10);
            var yearsOfService = totalServiceTime.Days / 365;
            if (yearsOfService < 10) return 10;
            if (yearsOfService < 20) return 15;
            return 20;
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
                LeaveUsages = CalculateLeaveDetails(personRoles[person.Id], leaveRequests[person.Id])
            }).ToList();
        }

        public void ThrowIfHrRequiredForUpdate(LeaveRequest updatedLeaveRequest, Guid? personMakingChanges)
        {
            LeaveRequest oldRequest = null;
            if (!updatedLeaveRequest.IsNew())
                oldRequest = _leaveRequestRepository.LeaveRequests.SingleOrDefault(request =>
                    request.Id == updatedLeaveRequest.Id);
            ThrowIfHrRequiredForUpdate(oldRequest, updatedLeaveRequest, personMakingChanges);
        }

        private IQueryable<PersonWithStaff> PeopleWithStaffUnderGroup(Guid orgGroupId)
        {
            return (from person in _personRepository.PeopleWithStaff
                from org in _orgGroupRepository.GetByIdWithChildren(orgGroupId)
                    .InnerJoin(orgGroup => orgGroup.Id == person.Staff.OrgGroupId)
                select person).OrderBy(_ => _.PreferredName ?? _.FirstName).ThenBy(_ => _.LastName);
        }

        public static void ThrowIfHrRequiredForUpdate(LeaveRequest oldRequest,
            LeaveRequest newRequest,
            Guid? personMakingChanges)
        {
            if (oldRequest == null)
            {
                if (newRequest.OverrideDays)
                {
                    throw new UnauthorizedAccessException(
                        "You're not allowed to override the leave calculation, talk to HR");
                }

                if (newRequest.Days != newRequest.CalculateLength() &&
                    newRequest.Days != newRequest.CalculateLength() - 0.5m)
                {
                    throw new ArgumentException(
                        $"Leave request days calculated didn't match what was expected for dates {newRequest.StartDate} to {newRequest.EndDate}");
                }

                return;
            }

            if (oldRequest.Approved != null)
            {
                throw new UnauthorizedAccessException("You're not allowed to modify approved leave requests");
            }

            if (oldRequest.PersonId != personMakingChanges ||
                oldRequest.PersonId != newRequest.PersonId)
            {
                throw new UnauthorizedAccessException("You aren't allowed to modify this leave request");
            }

            if (!oldRequest.OverrideDays && newRequest.OverrideDays)
            {
                throw new UnauthorizedAccessException(
                    "You aren't allowed to override the length of this leave request, talk to HR");
            }

            if (!newRequest.OverrideDays && newRequest.Days != newRequest.CalculateLength() &&
                newRequest.Days != newRequest.CalculateLength() - 0.5m)
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
                oldRequest.CalculateLength() != newRequest.CalculateLength())
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
    }
}