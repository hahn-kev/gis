using Backend.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Authorization
{
    public static class AuthorizationHelper
    {
        public static void AddMyAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(AddPolicies);
            services.AddScoped<IAuthorizationHandler, PersonEditAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, PersonEditAuthorizationHandlerLazy>();
            services.AddScoped<PersonEditAuthorizationHandlerLazy>();
            services.AddScoped<IAuthorizationHandler, StaffEditAuthorizationHandler>();
            services.AddScoped<IAuthorizationHandler, CanRequestLeaveHandler>();
            services.AddScoped<IAuthorizationHandler, CanRequestLeaveByPersonHandler>();
        }

        private static void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(nameof(MyPolicies.attachments),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.jobs),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.grades),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.role),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.evaluations),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.staff),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.holidays),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.contact),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.isSupervisor),
                builder => builder.RequireClaim(AuthenticateController.ClaimSupervisor));

            //leave
            options.AddPolicy(nameof(MyPolicies.leaveRequest),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.leaveSupervisor),
                builder => builder.RequireAssertion(context =>
                    context.User.IsSupervisor() || context.User.IsLeaveDelegate()));
            options.AddPolicy(nameof(MyPolicies.canRequestLeave),
                builder => builder.AddRequirements(new CanRequestLeaveRequirement()));

            options.AddPolicy(nameof(MyPolicies.training),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.orgGroup),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.endorsement),
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.hrSupervisorAdmin),
                builder => builder.RequireAssertion(
                    context => context.User.IsAdminOrHr() || context.User.IsSupervisor()));

            //edit requirements
            options.AddPolicy(nameof(MyPolicies.peopleEdit),
                policy => policy.AddRequirements(new PersonEditRequirement()));
            options.AddPolicy(nameof(MyPolicies.staffEdit),
                policy => policy.AddRequirements(new StaffEditRequirement()));

            options.AddPolicy(nameof(MyPolicies.people),
                builder => builder.RequireAssertion(context =>
                    context.User.IsInAnyRole("admin", "hr", "registrar") ||
                    context.User.IsSupervisor()));
            options.AddPolicy(nameof(MyPolicies.sendingOrg),
                builder => builder.RequireAssertion(context =>
                    context.User.IsInAnyRole("admin", "hr", "registrar") ||
                    context.User.IsHighLevelSupervisor()));
            options.AddPolicy(nameof(MyPolicies.orgTreeData),
                builder => builder.RequireAssertion(context =>
                    context.User.IsSupervisor() || context.User.IsAdminOrHr()));
        }
    }

    public enum MyPolicies
    {
        attachments,
        jobs,
        grades,
        role,
        evaluations,
        staff,
        holidays,
        contact,
        isSupervisor,
        leaveRequest,
        leaveSupervisor,
        training,
        orgGroup,
        endorsement,
        peopleEdit,
        staffEdit,
        people,
        sendingOrg,
        orgTreeData,
        hrSupervisorAdmin,
        canRequestLeave
    }
}