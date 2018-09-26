using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class LeaveRequestController : MyController
    {
        private readonly LeaveService _leaveService;
        private readonly IAuthorizationService _authorizationService;

        public LeaveRequestController(LeaveService leaveService, IAuthorizationService authorizationService)
        {
            _leaveService = leaveService;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "leaveRequest")]
        public IList<LeaveRequestWithNames> List()
        {
            return _leaveService.LeaveRequestsWithNames;
        }

        [HttpGet("mine")]
        public IList<LeaveRequestWithNames> ListMyLeave()
        {
            return _leaveService.ListByPersonId(User.PersonId() ??
                                                throw new UnauthorizedAccessException(
                                                    "Logged in user must be connected to a person, talk to HR about this issue"));
        }

        [HttpGet("supervisor")]
        [Authorize("leaveSupervisor")]
        public IList<LeaveRequestWithNames> Supervisor()
        {
            var groupId = User.LeaveDelegateGroupId() ?? User.SupervisorGroupId() ??
                          throw new UnauthorizedAccessException(
                              "Logged in user must be a supervisor or leave delegate");

            return _leaveService.ListUnderOrgGroup(groupId, User.PersonId());
        }

        [HttpGet("person/{personId}")]
        public IList<LeaveRequestWithNames> ListByPerson(Guid personId)
        {
            if ((!User.IsAdminOrHr() && !User.IsHighLevelSupervisor()) && User.PersonId() != personId)
                throw new UnauthorizedAccessException(
                    "You're only allowed to list your leave requests unless you're hr");
            return _leaveService.ListByPersonId(personId);
        }

        [HttpGet("supervisor/{supervisorId}")]
        public IList<LeaveRequestWithNames> ListBySupervisor(Guid supervisorId)
        {
            if ((!User.IsAdminOrHr() && !User.IsHighLevelSupervisor()) && User.PersonId() != supervisorId)
                throw new UnauthorizedAccessException(
                    "You can't view leave of another supervisor");
            return _leaveService.ListForSupervisor(supervisorId);
        }

        [HttpGet("{id}")]
        public LeaveRequestWithNames Get(Guid id)
        {
            return _leaveService.GetById(id);
        }

        [HttpPut]
        public LeaveRequest Update([FromBody] LeaveRequest updatedLeaveRequest)
        {
            if (!User.IsAdminOrHr())
            {
                _leaveService.ThrowIfHrRequiredForUpdate(updatedLeaveRequest, User.PersonId());
            }

            _leaveService.UpdateLeave(updatedLeaveRequest);

            return updatedLeaveRequest;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            if (User.IsAdminOrHr() ||
                (User.PersonId() != null && _leaveService.GetLeavePersonId(id) == User.PersonId()))
            {
                _leaveService.DeleteLeaveRequest(id);
            }
            else
            {
                throw new UnauthorizedAccessException("Logged in user isn't allowed to delete this leave request");
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RequestLeave([FromBody] LeaveRequest leaveRequest)
        {
            if (!await _leaveService.CanRequestLeave(User, leaveRequest))
            {
                throw new UnauthorizedAccessException("Logged in user isn't allowed to request leave for this person");
            }

            if (!User.IsAdminOrHr())
            {
                _leaveService.ThrowIfHrRequiredForUpdate(leaveRequest, User.PersonId());
            }

            Person notified = await _leaveService.RequestLeave(leaveRequest);
            return Json(notified);
        }

        [HttpGet("approve/{leaveRequestId}")]
        [Authorize("isSupervisor")]
        public IActionResult Approve(Guid leaveRequestId)
        {
            var personId = User.PersonId();
            if (personId == null)
                throw new UnauthorizedAccessException(
                    "Logged in user must be connected to a person, talk to HR about this issue");
            _leaveService.ApproveLeaveRequest(leaveRequestId, personId.Value);
            return this.ShowFrontendMessage("Leave request approved");
        }


        [HttpGet("people")]
        [Authorize(Policy = "leaveRequest")]
        public IList<PersonAndLeaveDetails> PeopleWithLeave(int year)
        {
            return _leaveService.PeopleWithLeave(year);
        }

        [HttpGet("people/supervisor")]
        [Authorize("leaveSupervisor")]
        public IList<PersonAndLeaveDetails> MyPeopleWithLeave(int year)
        {
            var groupId = User.LeaveDelegateGroupId() ?? User.SupervisorGroupId() ??
                          throw new UnauthorizedAccessException(
                              "Logged in user must be a supervisor or leave delegate");
            var people = _leaveService.PeopleInGroupWithLeave(groupId, year);

            return people;
        }
    }
}