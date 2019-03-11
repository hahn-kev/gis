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
        }

        private static void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy("attachments",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("jobs",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("grades",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("role",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("evaluations",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("staff",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("holidays",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("contact",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("isSupervisor",
                builder => builder.RequireClaim(AuthenticateController.ClaimSupervisor));
            options.AddPolicy("leaveRequest",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("leaveSupervisor",
                builder => builder.RequireAssertion(context =>
                    context.User.IsSupervisor() || context.User.IsLeaveDelegate()));

            options.AddPolicy("training",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("orgGroup",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("endorsement",
                builder => builder.RequireAssertion(context =>
                    context.User.IsAdminOrHr() || context.User.IsHighLevelSupervisor()));
            options.AddPolicy("peopleEdit", policy => policy.AddRequirements(new PersonEditRequirement()));
            options.AddPolicy("people",
                builder => builder.RequireAssertion(context =>
                    context.User.IsInAnyRole("admin", "hr", "registrar") ||
                    context.User.IsSupervisor()));
            options.AddPolicy("sendingOrg",
                builder => builder.RequireAssertion(context =>
                    context.User.IsInAnyRole("admin", "hr", "registrar") ||
                    context.User.IsHighLevelSupervisor()));
            options.AddPolicy("orgTreeData",
                builder => builder.RequireAssertion(context =>
                    context.User.IsSupervisor() || context.User.IsAdminOrHr()));
        }
    }
}