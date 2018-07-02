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

        public LeaveRequestController(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        [Authorize(Policy = "leaveRequest")]
        public IList<LeaveRequestWithNames> List()
        {
            return _leaveService.LeaveRequestsWithNames;
        }

        [HttpGet("person/{personId}")]
        public IList<LeaveRequestWithNames> ListByPerson(Guid personId)
        {
            if (!User.IsAdminOrHr() && User.PersonId() != personId)
                throw new UnauthorizedAccessException(
                    "You're only allowed to list your leave requests unless you're hr");
            return _leaveService.ListByPersonId(personId);
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
            if (!_leaveService.CanRequestLeave(User, leaveRequest))
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
        public IList<PersonAndLeaveDetails> PeopleWithLeave()
        {
            return _leaveService.PeopleWithCurrentLeave(null);
        }
    }
}