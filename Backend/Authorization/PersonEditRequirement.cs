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

    public class PersonEditAuthorizationHandler : AuthorizationHandler<PersonEditRequirement, Guid>
    {
        private readonly OrgGroupService _orgGroupService;

        public PersonEditAuthorizationHandler(OrgGroupService orgGroupService)
        {
            _orgGroupService = orgGroupService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PersonEditRequirement requirement,
            Guid personId)
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

            if (await _orgGroupService.IsPersonInGroup(personId, supervisorGroupId))
            {
                context.Succeed(requirement);
            }
        }
    }
}