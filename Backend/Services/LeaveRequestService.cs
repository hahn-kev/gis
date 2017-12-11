using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class LeaveRequestService
    {
        private readonly OrgGroupRepository _orgGroupRepository;

        private readonly PersonRepository _personRepository;
        private readonly LeaveRequestRepository _leaveRequestRepository;
        private readonly IEmailService _emailService;
        private readonly Settings _settings;
        private readonly IEntityService _entityService;
        private readonly UsersRepository _usersRepository;

        public LeaveRequestService(OrgGroupRepository orgGroupRepository,
            PersonRepository personRepository,
            IEmailService emailService,
            IOptions<Settings> options,
            LeaveRequestRepository leaveRequestRepository,
            IEntityService entityService,
            UsersRepository usersRepository)
        {
            _orgGroupRepository = orgGroupRepository;
            _personRepository = personRepository;
            _emailService = emailService;
            _leaveRequestRepository = leaveRequestRepository;
            _entityService = entityService;
            _usersRepository = usersRepository;
            _settings = options.Value;
        }

        public IList<LeaveRequestWithNames> LeaveRequestsWithNames => _leaveRequestRepository.LeaveRequestWithNames.ToList();

        private IQueryable<OrgGroupWithSupervisor> OrgGroups => _orgGroupRepository.OrgGroupsWithSupervisor;

        public bool ApproveLeaveRequest(Guid id, int loggedInUser)
        {
            //todo fix how we get from logged in user to personId
            var user = _usersRepository.UserById(loggedInUser);
            var superviserId = _personRepository.PeopleExtended.Where(person => person.Email == user.Email)
                .Select(extended => extended.Id).First();
            return _leaveRequestRepository.ApproveLeaveRequest(id, superviserId);
        }

        public async Task<Person> RequestLeave(LeaveRequest leaveRequest)
        {
            var result =
            (from personOnLeave in _personRepository.PeopleExtended.Where(person => person.Id == leaveRequest.PersonId)
                from department in OrgGroups.InnerJoin(@group =>
                    @group.Id == personOnLeave.OrgGroupId || @group.Supervisor == personOnLeave.Id)
                from devision in OrgGroups.LeftJoin(@group => @group.Id == department.ParentId)
                from supervisorGroup in OrgGroups.LeftJoin(@group => @group.Id == devision.ParentId)
                select new
                {
                    personOnLeave,
                    department,
                    devision,
                    supervisorGroup
                }).FirstOrDefault();

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
            if (requestedBy.Id == orgGroup.Supervisor) return null;
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
    }
}