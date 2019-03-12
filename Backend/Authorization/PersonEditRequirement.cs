using System;
using System.Threading.Tasks;
using Backend.Controllers;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Authorization
{
    public class PersonEditRequirement : IAuthorizationRequirement
    {
    }

    public class StaffEditRequirement : IAuthorizationRequirement
    {
    }

    public class PersonEditAuthorizationHandler : AuthorizationHandler<PersonEditRequirement, Guid>
    {
        private readonly PersonEditAuthorizationHandlerLazy _handlerLazy;

        public PersonEditAuthorizationHandler(PersonEditAuthorizationHandlerLazy handlerLazy)
        {
            _handlerLazy = handlerLazy;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PersonEditRequirement requirement,
            Guid resource)
        {
            return _handlerLazy.HandlePersonRequirement(context, requirement, () => resource);
        }
    }

    public class PersonEditAuthorizationHandlerLazy : AuthorizationHandler<PersonEditRequirement, Func<Guid>>
    {
        private readonly OrgGroupService _orgGroupService;

        public PersonEditAuthorizationHandlerLazy(OrgGroupService orgGroupService)
        {
            _orgGroupService = orgGroupService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PersonEditRequirement requirement,
            Func<Guid> personId)
        {
            return HandlePersonRequirement(context, requirement, personId);
        }

        internal async Task HandlePersonRequirement(AuthorizationHandlerContext context,
            PersonEditRequirement requirement,
            Func<Guid> personId)
        {
            if (context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor() ||
                context.User.IsInRole("registrar"))
            {
                context.Succeed(requirement);
                return;
            }

            var supervisorGroupId = context.User.SupervisorGroupId() ?? Guid.Empty;
            if (!context.User.IsSupervisor() || supervisorGroupId == Guid.Empty)
            {
                context.Fail();
                return;
            }

            if (await _orgGroupService.IsPersonInGroup(personId(), supervisorGroupId))
            {
                context.Succeed(requirement);
            }
        }
    }

    public class StaffEditAuthorizationHandler : AuthorizationHandler<StaffEditRequirement, Guid>
    {
        private readonly OrgGroupService _orgGroupService;

        public StaffEditAuthorizationHandler(OrgGroupService orgGroupService)
        {
            _orgGroupService = orgGroupService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            StaffEditRequirement requirement,
            Guid staffId)
        {
            if (context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor() ||
                context.User.IsInRole("registrar"))
            {
                context.Succeed(requirement);
                return;
            }

            var supervisorGroupId = context.User.SupervisorGroupId() ?? Guid.Empty;
            if (!context.User.IsSupervisor() || supervisorGroupId == Guid.Empty)
            {
                context.Fail();
                return;
            }

            if (await _orgGroupService.IsStaffInGroup(staffId, supervisorGroupId))
            {
                context.Succeed(requirement);
            }
        }
    }
}