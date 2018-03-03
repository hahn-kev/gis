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
            return _leaveRequestRepository.LeaveRequestWithNames.Where(request => request.PersonId == personId)
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
            if (result.personOnLeave?.StaffId == null) throw new Exception("Person requesting leave must be staff");
            leaveRequest.Approved = null;
            leaveRequest.ApprovedById = null;
            leaveRequest.CreatedDate = DateTime.Now;
            _entityService.Save(leaveRequest);
            try
            {
                return await ResolveLeaveRequestChain(leaveRequest,
                    result.personOnLeave,
                    result.department,
                    result.devision,
                    result.supervisorGroup);
            }
            catch
            {
                _entityService.Delete(leaveRequest);
                throw;
            }
        }

        public async Task<PersonExtended> ResolveLeaveRequestChain(LeaveRequest leaveRequest,
            PersonExtended requestedBy,
            OrgGroupWithSupervisor department,
            OrgGroupWithSupervisor devision,
            OrgGroupWithSupervisor supervisorGroup)
        {
            return await DoNotifyWork(leaveRequest, requestedBy, department) ??
                   await DoNotifyWork(leaveRequest, requestedBy, devision) ??
                   await DoNotifyWork(leaveRequest, requestedBy, supervisorGroup);
        }

        private async ValueTask<PersonExtended> DoNotifyWork(LeaveRequest leaveRequest,
            PersonExtended requestedBy,
            OrgGroupWithSupervisor orgGroup)
        {
            //super and requested by will be the same if the requester is a supervisor
            if (orgGroup == null || requestedBy.Id == orgGroup.Supervisor) return null;
            if (orgGroup.ApproverIsSupervisor && orgGroup.SupervisorPerson != null)
            {
                await SendRequestApproval(leaveRequest, requestedBy, orgGroup.SupervisorPerson);
                return orgGroup.SupervisorPerson;
            }

            if (orgGroup.SupervisorPerson != null)
            {
                await NotifyOfLeaveRequest(leaveRequest, requestedBy, orgGroup.SupervisorPerson);
            }

            return null;
        }

        private async Task NotifyOfLeaveRequest(LeaveRequest leaveRequest,
            PersonExtended requestedBy,
            PersonExtended supervisor)
        {
            var leaveTimespan = (leaveRequest.StartDate - leaveRequest.EndDate).Duration();
            var substituions = new Dictionary<string, string>
            {
                {":firstName", supervisor.FirstName},
                {":requester", requestedBy.FirstName},
                {":time", $"{leaveTimespan.Days} Day(s)"}
            };
            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} has requested leave",
                EmailService.Template.NotifyLeaveRequest,
                requestedBy,
                supervisor);
        }

        private async Task SendRequestApproval(LeaveRequest leaveRequest,
            PersonExtended requestedBy,
            PersonExtended supervisor)
        {
            var leaveTimespan = (leaveRequest.StartDate - leaveRequest.EndDate).Duration();
            var substituions = new Dictionary<string, string>
            {
                {":approve", $"{_settings.BaseUrl}/api/leaveRequest/approve/{leaveRequest.Id}"},
                {":firstName", supervisor.FirstName},
                {":requester", requestedBy.FirstName},
                {":time", $"{leaveTimespan.Days} Day(s)"}
            };

            await _emailService.SendTemplateEmail(substituions,
                $"{requestedBy.PreferredName} Leave request approval",
                EmailService.Template.RequestLeaveApproval,
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
                _personRepository.GetPersonRoles(personId),
                DateTime.Now.SchoolYear());
        }

        public LeaveDetails GetLeaveDetails(Guid personId, IEnumerable<PersonRole> personRoles, int schoolYear)
        {
            var leaveRequests = _personRepository.LeaveRequests
                .Where(request => request.PersonId == personId && request.StartDate.InSchoolYear(schoolYear));
            return new LeaveDetails
            {
                LeaveUseages = CalculateLeaveDetails(personRoles, leaveRequests)
            };
        }

        private List<LeaveUseage> CalculateLeaveDetails(IEnumerable<PersonRole> personRoles,
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
                        : LeaveAllowed(type, Enumerable.Empty<PersonRole>())
                }
            ).Where(useage => useage.TotalAllowed != null).ToList();
        }

        public static int TotalLeaveUsed(IEnumerable<LeaveRequest> requests)
        {
            return requests.Sum(request => request.StartDate.BusinessDaysUntil(request.EndDate));
        }

        public static int? LeaveAllowed(LeaveType leaveType, IEnumerable<PersonRole> personRoles)
        {
            switch (leaveType)
            {
                case LeaveType.Sick: return 30;
                case LeaveType.Personal: return 5;
                case LeaveType.Funeral: return 5;
                case LeaveType.Maternity: return 90;
                case LeaveType.Paternity: return 5;
                case LeaveType.Buisness: return 0;
                case LeaveType.Vacation: break;
                default: return null;
            }

            //calculation for vacation time is done here
            var totalServiceTime = TimeSpan.Zero;
            foreach (var role in personRoles)
            {
                if (role.IsDirectorPosition && role.Active) return 20;
                if (role.IsStaffPosition || role.IsDirectorPosition)
                    totalServiceTime = totalServiceTime + role.LengthOfService();
            }

            //no time has been spent as staff or a director, therefore no vacation time is allowed
            if (totalServiceTime == TimeSpan.Zero) return null;
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
            var personRoles = _personRepository.PersonRoles.Where(role => peopleIds.Contains(role.PersonId))
                .ToLookup(role => role.PersonId);
            return
                people.Select(person => new PersonAndLeaveDetails
                {
                    Person = person,
                    LeaveUseages = CalculateLeaveDetails(personRoles[person.Id], leaveRequests[person.Id])
                }).ToList();
        }
    }
}