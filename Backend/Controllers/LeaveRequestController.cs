using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly LeaveRequestService _leaveRequestService;
        private readonly ClaimsIdentityOptions _identityOptions;

        public LeaveRequestController(LeaveRequestService leaveRequestService,
            IOptions<ClaimsIdentityOptions> identityOptions)
        {
            _leaveRequestService = leaveRequestService;
            _identityOptions = identityOptions.Value;
        }

        [HttpGet]
        [Authorize(Roles = "admin,hr")]
        public IList<LeaveRequestWithNames> List()
        {
            return _leaveRequestService.LeaveRequestsWithNames;
        }

        [HttpGet("person/{personId}")]
        public IList<LeaveRequestWithNames> ListByPerson(Guid personId)
        {
            if (!User.IsAdminOrHr() && User.PersonId() != personId)
                throw new UnauthorizedAccessException(
                    "You're only allowed to list your leave requests unless you're hr");
            return _leaveRequestService.ListByPersonId(personId);
        }

        [HttpGet("{id}")]
        public LeaveRequestWithNames Get(Guid id)
        {
            return _leaveRequestService.GetById(id);
        }

        [HttpPut]
        public IActionResult Update([FromBody] LeaveRequest leaveRequest)
        {
            if (leaveRequest.Id == Guid.Empty)
                throw new Exception("Trying to create a new request with the update action, use post instead");
            if (User.IsAdminOrHr() ||
                (User.PersonId() != null && _leaveRequestService.GetLeavePersonId(leaveRequest.Id) == User.PersonId()))
            {
                _leaveRequestService.UpdateLeave(leaveRequest);
            }
            else
            {
                throw new UnauthorizedAccessException("Logged in user isn't allowed to modify this leave request");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            if (User.IsAdminOrHr() ||
                (User.PersonId() != null && _leaveRequestService.GetLeavePersonId(id) == User.PersonId()))
            {
                _leaveRequestService.DeleteLeaveRequest(id);
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
            if (!_leaveRequestService.CanRequestLeave(User, leaveRequest))
            {
                throw new UnauthorizedAccessException("Logged in user isn't allowed to request leave for this person");
            }

            Person notified = await _leaveRequestService.RequestLeave(leaveRequest);
            return Json(notified);
        }

        [HttpGet("approve/{leaveRequestId}")]
        public IActionResult Approve(Guid leaveRequestId)
        {
            //todo validate that logged in user is HR/ADMIN or is the supervisor of the person who created the leave request
            var personId = User.PersonId();
            if (personId == null)
                throw new UnauthorizedAccessException(
                    "Logged in user must be connected to a person talk to HR about this issue");
            _leaveRequestService.ApproveLeaveRequest(leaveRequestId, personId.Value);
            return this.ShowFrontendMessage("Leave request approved");
        }
    }
}