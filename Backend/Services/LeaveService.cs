using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Utils;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class LeaveService
    {
        private readonly OrgGroupRepository _orgGroupRepository;

        private readonly PersonRepository _personRepository;
        private readonly LeaveRequestRepository _leaveRequestRepository;
        private readonly IEmailService _emailService;
        private readonly Settings _settings;
        private readonly IEntityService _entityService;
        private readonly IAuthorizationService _authorizationService;

        public LeaveService(OrgGroupRepository orgGroupRepository,
            PersonRepository personRepository,
            IEmailService emailService,
            IOptions<Settings> options,
            LeaveRequestRepository leaveRequestRepository,
            IEntityService entityService, IAuthorizationService authorizationService)
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
            _emailService = emailService;
            _leaveRequestRepository = leaveRequestRepository;
            _entityService = entityService;
            _authorizationService = authorizationService;
            _settings = options.Value;
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

        public bool ApproveLeaveRequest(Guid leaveRequestId, Guid personId)
        {
            //ensure that this id is a valid person before we approve it
            var superviserId = _personRepository.PeopleExtended.Where(person => person.Id == personId)
                .Select(extended => extended.Id).First();
            return _leaveRequestRepository.ApproveLeaveRequest(leaveRequestId, superviserId);
        }

        public async Task<Person> RequestLeave(LeaveRequest leaveRequest)
        {
            var result = _orgGroupRepository.PersonWithOrgGroupChain(leaveRequest.PersonId);
            if (result.personOnLeave?.StaffId == null)
                throw new UnauthorizedAccessException("Person requesting leave must be staff");
            leaveRequest.Approved = null;
            leaveRequest.ApprovedById = null;
            var leaveUsage = GetLeaveUseage(leaveRequest.Type, result.personOnLeave.Id,
                leaveRequest.StartDate.SchoolYear());
            var isNew = leaveRequest.IsNew();
            _entityService.Save(leaveRequest);
            try
            {
                await NotifyHr(leaveRequest, result.personOnLeave, leaveUsage);
                return await ResolveLeaveRequestChain(leaveRequest,
                    result.personOnLeave,
                    result.department,
                    result.devision,
                    result.supervisorGroup,
                    leaveUsage);
            }
            catch
            {
                if (isNew)
                    _entityService.Delete(leaveRequest);
                throw;
            }
        }

        public async Task<PersonExtended> ResolveLeaveRequestChain(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup,
            LeaveUsage leaveUsage)
        {
            return await DoNotifyWork(leaveRequest, requestedBy, department, leaveUsage) ??
                   await DoNotifyWork(leaveRequest, requestedBy, devision, leaveUsage) ??
                   await DoNotifyWork(leaveRequest, requestedBy, supervisorGroup, leaveUsage);
        }

        private async ValueTask<PersonWithStaff> DoNotifyWork(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor orgGroup,
            LeaveUsage leaveUsage)
        {
            //super and requested by will be the same if the requester is a supervisor
            if (orgGroup == null || requestedBy.Id == orgGroup.Supervisor) return null;
            if (orgGroup.ApproverIsSupervisor && orgGroup.SupervisorPerson != null)
            {
                await SendRequestApproval(leaveRequest, requestedBy, orgGroup.SupervisorPerson, leaveUsage);
                return orgGroup.SupervisorPerson;
            }

            if (orgGroup.SupervisorPerson != null)
            {
                await NotifyOfLeaveRequest(leaveRequest, requestedBy, orgGroup.SupervisorPerson, leaveUsage);
            }

            return null;
        }

        private async Task NotifyOfLeaveRequest(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor,
            LeaveUsage leaveUsage)
        {
            //this is a list of substitutions avalible in the email template
            //these are used when notifying non approving supervisors of leave
            //$LEAVE-SUBSTITUTIONS$
            var substituions = new Dictionary<string, string>
            {
                {":type", leaveRequest.Type.ToString()},
                {":firstName", supervisor.PreferredName + " " + supervisor.LastName},
                {":requester", requestedBy.PreferredName + " " + requestedBy.LastName},
                {":start", leaveRequest.StartDate.ToString("MMM d yyyy")},
                {":end", leaveRequest.EndDate.ToString("MMM d yyyy")},
                {":time", $"{leaveRequest.Days} Day(s)"},
                {":left", $"{leaveUsage.Left} Day(s)"}
            };
            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} has requested leave",
                EmailTemplate.NotifyLeaveRequest,
                requestedBy,
                supervisor);
        }

        private async Task NotifyHr(LeaveRequest leaveRequest, PersonWithStaff requestedBy, LeaveUsage leaveUsage)
        {
            if (!ShouldNotifyHr(leaveRequest, leaveUsage)) return;
            //this is a list of substitutions avalible in the email template
            //these are used when notifying HR of leave
            //$LEAVE-SUBSTITUTIONS$
            var substituions = new Dictionary<string, string>
            {
                {":type", leaveRequest.Type.ToString()},
                {":requester", requestedBy.PreferredName + " " + requestedBy.LastName},
                {":start", leaveRequest.StartDate.ToString("MMM d yyyy")},
                {":end", leaveRequest.EndDate.ToString("MMM d yyyy")},
                {":time", $"{leaveRequest.Days} Day(s)"},
                {":left", $"{leaveUsage.Left} Day(s)"}
            };
            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} has requested leave",
                EmailTemplate.NotifyHrLeaveRequest,
                requestedBy,
                _personRepository.GetStaffNotifyHr());
        }

        public bool ShouldNotifyHr(LeaveRequest leaveRequest, LeaveUsage leaveUsage)
        {
            if (leaveRequest.Type == LeaveType.Other) return true;
            return leaveUsage.Left < leaveRequest.Days;
        }

        private async Task SendRequestApproval(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor,
            LeaveUsage leaveUsage)
        {
            //this is a list of substitutions avalible in the email template
            //these are used when notifying the approving supervisor of leave
            //$LEAVE-SUBSTITUTIONS$
            var substituions = new Dictionary<string, string>
            {
                {":type", leaveRequest.Type.ToString()},
                {":approve", $"{_settings.BaseUrl}/api/leaveRequest/approve/{leaveRequest.Id}"},
                {":supervisor", supervisor.PreferredName + " " + supervisor.LastName},
                {":firstName", supervisor.PreferredName + " " + supervisor.LastName},
                {":requester", requestedBy.PreferredName + " " + requestedBy.LastName},
                {":start", leaveRequest.StartDate.ToString("MMM d yyyy")},
                {":end", leaveRequest.EndDate.ToString("MMM d yyyy")},
                {":time", $"{leaveRequest.Days} Day(s)"},
                {":left", $"{leaveUsage.Left} Day(s)"},
                {":totalDays", $"{leaveUsage.TotalAllowed} Day(s)"},
                {":reason", leaveRequest.Reason}
            };

            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} Leave request approval",
                EmailTemplate.RequestLeaveApproval,
                requestedBy,
                supervisor);
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
            return PeopleWithLeave(_personRepository.PeopleWithStaff.Where(staff => staff.StaffId != null), schoolYear);
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
            return from person in _personRepository.PeopleWithStaff
                from org in _orgGroupRepository.GetByIdWithChildren(orgGroupId)
                    .InnerJoin(orgGroup => orgGroup.Id == person.Staff.OrgGroupId)
                select person;
        }

        public void ThrowIfHrRequiredForUpdate(LeaveRequest oldRequest,
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