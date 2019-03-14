using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Backend.Authorization;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize("leaveRequest")]
        public IList<LeaveRequestWithNames> List()
        {
            return _leaveService.LeaveRequestsWithNames;
        }

        [HttpGet("public")]
        [Authorize]
        public List<LeaveRequestPublic> Public()
        {
            return _leaveService.PublicLeaveRequests();
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
        public Task<ActionResult<IList<LeaveRequestWithNames>>> ListByPerson(Guid personId)
        {
            return TryExecute(MyPolicies.peopleEdit,
                personId,
                () => _leaveService.ListByPersonId(personId));
        }

        [HttpGet("supervisor/{supervisorId}")]
        [Authorize("leaveRequest")]
        public IList<LeaveRequestWithNames> ListBySupervisor(Guid supervisorId)
        {
            return _leaveService.ListForSupervisor(supervisorId);
        }

        [HttpGet("{id}")]
        public LeaveRequestWithNames Get(Guid id)
        {
            return _leaveService.GetById(id);
        }

        [HttpPut]
        public Task<ActionResult<LeaveRequest>> Update([FromBody] LeaveRequest updatedLeaveRequest)
        {
            return TryExecute(MyPolicies.canRequestLeave,
                updatedLeaveRequest,
                () =>
                {
                    if (!User.IsAdminOrHr())
                    {
                        _leaveService.ThrowIfHrRequiredForUpdate(updatedLeaveRequest);
                    }

                    _leaveService.UpdateLeave(updatedLeaveRequest);

                    return updatedLeaveRequest;
                });
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
        public Task<ActionResult<Person>> RequestLeave([FromBody] LeaveRequest leaveRequest)
        {
            return TryExecute(MyPolicies.canRequestLeave,
                leaveRequest,
                () =>
                {
                    if (!User.IsAdminOrHr())
                    {
                        _leaveService.ThrowIfHrRequiredForUpdate(leaveRequest);
                    }

                    return _leaveService.RequestLeave(leaveRequest);
                });
        }

        [HttpGet("approve/{leaveRequestId}")]
        [Authorize("isSupervisor")]
        public Task<ActionResult> Approve(Guid leaveRequestId)
        {
            var personId = User.PersonId();
            if (personId == null)
                throw new UnauthorizedAccessException(
                    "Logged in user must be connected to a person, talk to HR about this issue");
            return TryExecute<Func<Guid>>(MyPolicies.peopleEdit,
                () => _leaveService.GetLeavePersonId(leaveRequestId) ??
                      throw new ArgumentException("Unable to find leave request, it may have been deleted"),
                async () =>
                {
                    var (_, requester, notified) =
                        await _leaveService.ApproveLeaveRequest(leaveRequestId, personId.Value);
                    if (notified)
                    {
                        return this.ShowFrontendMessage(
                            $"Leave request approved{Environment.NewLine}{requester.PreferredName ?? requester.FirstName} has been notified");
                    }

                    return this.ShowFrontendMessage(
                        $"Leave request approved{Environment.NewLine}{requester.PreferredName ?? requester.FirstName} does not have an email and has not been notified");
                });
        }


        [HttpGet("people")]
        [Authorize("leaveRequest")]
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
            var personId = User.PersonId();
            if (personId != null && people.All(details => details.Person.Id != personId))
            {
                people.Insert(0, _leaveService.PersonWithLeave(personId.Value, year));
            }

            return people;
        }

        [HttpGet("people/mine")]
        public IList<PersonAndLeaveDetails> MyLeaveDetails(int year)
        {
            var personId = User.PersonId() ??
                           throw new AuthenticationException("User must be a person to request leave");
            var people = new List<PersonAndLeaveDetails>
            {
                _leaveService.PersonWithLeave(personId, year)
            };

            return people;
        }
    }
}