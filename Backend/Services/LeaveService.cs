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

        public LeaveService(OrgGroupRepository orgGroupRepository,
            PersonRepository personRepository,
            IEmailService emailService,
            IOptions<Settings> options,
            LeaveRequestRepository leaveRequestRepository,
            IEntityService entityService)
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
            _emailService = emailService;
            _leaveRequestRepository = leaveRequestRepository;
            _entityService = entityService;
            _settings = options.Value;
        }

        public IList<LeaveRequestWithNames> LeaveRequestsWithNames =>
            _leaveRequestRepository.LeaveRequestWithNames.ToList();

        public IList<LeaveRequestWithNames> ListByPersonId(Guid personId)
        {
            return _leaveRequestRepository.LeaveRequestWithNames
                .Where(request => request.PersonId == personId)
                .OrderBy(leaveRequest => leaveRequest.StartDate)
                .ToList();
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
            var leaveUsage = GetCurrentLeaveUseage(leaveRequest.Type, result.personOnLeave.Id);
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
            LeaveUseage leaveUseage)
        {
            return await DoNotifyWork(leaveRequest, requestedBy, department, leaveUseage) ??
                   await DoNotifyWork(leaveRequest, requestedBy, devision, leaveUseage) ??
                   await DoNotifyWork(leaveRequest, requestedBy, supervisorGroup, leaveUseage);
        }

        private async ValueTask<PersonWithStaff> DoNotifyWork(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            OrgGroupWithSupervisor orgGroup, LeaveUseage leaveUseage)
        {
            //super and requested by will be the same if the requester is a supervisor
            if (orgGroup == null || requestedBy.Id == orgGroup.Supervisor) return null;
            if (orgGroup.ApproverIsSupervisor && orgGroup.SupervisorPerson != null)
            {
                await SendRequestApproval(leaveRequest, requestedBy, orgGroup.SupervisorPerson, leaveUseage);
                return orgGroup.SupervisorPerson;
            }

            if (orgGroup.SupervisorPerson != null)
            {
                await NotifyOfLeaveRequest(leaveRequest, requestedBy, orgGroup.SupervisorPerson, leaveUseage);
            }

            return null;
        }

        private async Task NotifyOfLeaveRequest(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor, LeaveUseage leaveUseage)
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
                {":left", $"{leaveUseage.Left} Day(s)"}
            };
            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} has requested leave",
                EmailTemplate.NotifyLeaveRequest,
                requestedBy,
                supervisor);
        }

        private async Task NotifyHr(LeaveRequest leaveRequest, PersonWithStaff requestedBy, LeaveUseage leaveUseage)
        {
            if (!ShouldNotifyHr(leaveRequest, leaveUseage)) return;
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
                {":left", $"{leaveUseage.Left} Day(s)"}
            };
            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} has requested leave",
                EmailTemplate.NotifyHrLeaveRequest,
                requestedBy,
                _personRepository.GetHrAdminStaff());
        }

        public bool ShouldNotifyHr(LeaveRequest leaveRequest, LeaveUseage leaveUseage)
        {
            if (leaveRequest.Type == LeaveType.Other) return true;
            return leaveUseage.Left < leaveRequest.Days;
        }

        private async Task SendRequestApproval(LeaveRequest leaveRequest,
            PersonWithStaff requestedBy,
            PersonWithStaff supervisor,
            LeaveUseage leaveUseage)
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
                {":left", $"{leaveUseage.Left} Day(s)"},
                {":totalDays", $"{leaveUseage.TotalAllowed} Day(s)"},
                {":reason", leaveRequest.Reason}
            };

            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} Leave request approval",
                EmailTemplate.RequestLeaveApproval,
                requestedBy,
                supervisor);
        }

        public bool CanRequestLeave(ClaimsPrincipal user, LeaveRequest leaveRequest)
        {
            return leaveRequest.PersonId == user.PersonId() ||
                   user.IsAdminOrHr();
        }

        public LeaveDetails GetCurrentLeaveDetails(PersonWithOthers person)
        {
            return GetLeaveDetails(person.Id, person.Roles, DateTime.Now.SchoolYear());
        }

        public LeaveDetails GetCurrentLeaveDetails(Guid personId)
        {
            return GetLeaveDetails(personId,
                _personRepository.GetPersonRolesWithJob(personId),
                DateTime.Now.SchoolYear());
        }

        public LeaveDetails GetLeaveDetails(Guid personId, IEnumerable<PersonRoleWithJob> personRoles, int schoolYear)
        {
            var leaveRequests = _personRepository.LeaveRequests
                .Where(request => request.PersonId == personId && request.StartDate.InSchoolYear(schoolYear));
            return new LeaveDetails
            {
                LeaveUseages = CalculateLeaveDetails(personRoles, leaveRequests)
            };
        }

        private List<LeaveUseage> CalculateLeaveDetails(IEnumerable<PersonRoleWithJob> personRoles,
            IEnumerable<LeaveRequest> leaveRequests)
        {
            var leaveTypes = Enum.GetValues(typeof(LeaveType)).Cast<LeaveType>();
            //we do this here so we can make personRoles enumerable
            var vacationAllowed = LeaveAllowed(LeaveType.Vacation, personRoles);
            return leaveTypes.GroupJoin(leaveRequests,
                type => type,
                request => request.Type,
                (type, requests) => new LeaveUseage
                {
                    LeaveType = type,
                    Used = TotalLeaveUsed(requests),
                    TotalAllowed = type == LeaveType.Vacation
                        ? vacationAllowed
                        : LeaveAllowed(type, Enumerable.Empty<PersonRoleWithJob>())
                }
            ).ToList();
        }

        public LeaveUseage GetCurrentLeaveUseage(LeaveType leaveType, Guid personId)
        {
            return GetLeaveUseage(leaveType, personId, DateTime.Now.SchoolYear());
        }

        private LeaveUseage GetLeaveUseage(LeaveType leaveType, Guid personId, int schoolYear)
        {
            return new LeaveUseage
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
            return _personRepository.LeaveRequests
                .Where(request => request.PersonId == personId
                                  && request.StartDate.InSchoolYear(schoolYear)
                                  && request.Type == leaveType)
                .Sum(request => request.Days);
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
            foreach (var role in personRoles)
            {
                if (role.Job.OrgGroup?.Type != GroupType.Department && role.Job.OrgGroup?.Supervisor == role.PersonId &&
                    role.Active) return 20;
                if (role.Job.Status == JobStatus.FullTime || role.Job.Status == JobStatus.HalfTime)
                    totalServiceTime = totalServiceTime + role.LengthOfService();
            }

            //no time has been spent as staff or a director, therefore no vacation time is allowed
            if (totalServiceTime == TimeSpan.Zero) return 0;
            var yearsOfService = totalServiceTime.Days / 365;
            if (yearsOfService < 10) return 10;
            if (yearsOfService < 20) return 15;
            return 20;
        }

        public IList<PersonAndLeaveDetails> PeopleWithLeave(Guid? personId)
        {
            if (personId != null)
            {
                var person = _personRepository.People.SingleOrDefault(p => p.Id == personId && p.StaffId != null);
                if (person == null) return new List<PersonAndLeaveDetails>();
                return new List<PersonAndLeaveDetails>
                {
                    new PersonAndLeaveDetails
                    {
                        Person = person,
                        LeaveUseages = GetCurrentLeaveDetails(personId.Value).LeaveUseages
                    }
                };
            }

            var people = _personRepository.People.Where(person => person.StaffId != null).ToList();
            var peopleIds = people.Select(person => person.Id).ToList();
            var leaveRequests = _personRepository.LeaveRequests.Where(request => peopleIds.Contains(request.PersonId))
                .ToLookup(request => request.PersonId);
            var personRoles = _personRepository.PersonRolesWithJob.Where(role => peopleIds.Contains(role.PersonId))
                .ToLookup(role => role.PersonId);
            return
                people.Select(person => new PersonAndLeaveDetails
                {
                    Person = person,
                    LeaveUseages = CalculateLeaveDetails(personRoles[person.Id], leaveRequests[person.Id])
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
                    throw new ArgumentException($"Leave request days calculated didn't match what was expected for dates {newRequest.StartDate} to {newRequest.EndDate}");
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