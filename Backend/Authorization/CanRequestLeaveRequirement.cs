using System;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Entities;
using Backend.Services;
using LinqToDB;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization
{
    public class CanRequestLeaveRequirement : IAuthorizationRequirement
    {
    }

    public class CanRequestLeaveHandler : AuthorizationHandler<CanRequestLeaveRequirement, LeaveRequest>
    {
        private readonly LeaveService _leaveService;

        public CanRequestLeaveHandler(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanRequestLeaveRequirement requirement,
            LeaveRequest leaveRequest)
        {
            var user = context.User;
            if (leaveRequest.PersonId == user.PersonId() || user.IsAdminOrHr() || user.IsHighLevelSupervisor())
            {
                context.Succeed(requirement);
                return;
            }

            //todo support both?
            var groupId = user.LeaveDelegateGroupId() ?? user.SupervisorGroupId();
            if (groupId == null)
            {
                return;
            }

            bool isSupervisor = await _leaveService.PeopleWithStaffUnderGroup(groupId.Value)
                .AnyAsync(person => person.Id == leaveRequest.PersonId);
            if (isSupervisor) context.Succeed(requirement);
        }
    }

    public class CanRequestLeaveByPersonHandler : AuthorizationHandler<CanRequestLeaveRequirement, Guid>
    {
        private readonly LeaveService _leaveService;

        public CanRequestLeaveByPersonHandler(LeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CanRequestLeaveRequirement requirement,
            Guid personId)
        {
            var user = context.User;
            if (personId == user.PersonId() || user.IsAdminOrHr() || user.IsHighLevelSupervisor())
            {
                context.Succeed(requirement);
                return;
            }

            //todo support both?
            var groupId = user.LeaveDelegateGroupId() ?? user.SupervisorGroupId();
            if (groupId == null)
            {
                return;
            }

            bool isSupervisor = await _leaveService.PeopleWithStaffUnderGroup(groupId.Value)
                .AnyAsync(person => person.Id == personId);
            if (isSupervisor) context.Succeed(requirement);
        }
    }
}