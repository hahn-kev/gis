using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Backend.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Backend.Controllers
{
    public static class ControllerExtensions
    {
        public static ArgumentException Errors(this IdentityResult result)
        {
            return new ArgumentException(string.Join(Environment.NewLine,
                result.Errors
                    .Select(x => x.Description)));
        }

        public static ActionResult ShowFrontendMessage(this Controller controller, string message)
        {
            return controller.Redirect(RedirectFrontendMessagePath(message));
        }

        public static string RedirectFrontendMessagePath(string message)
        {
            return $"~/message?text={Uri.EscapeDataString(message)}";
        }

        public static string RedirectLogin(string postLoginRedirect)
        {
            return $"/login?redirect={Uri.EscapeDataString(postLoginRedirect)}";
        }

        public static Guid? PersonId(this ClaimsPrincipal user)
        {
            return user.ClaimValueAsGuid(AuthenticateController.ClaimPersonId);
        }

        public static bool IsStaff(this ClaimsPrincipal user)
        {
            return user.PersonId() != null;
        }

        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            return roles.Any(user.IsInRole);
        }

        public static bool IsAdminOrHr(this ClaimsPrincipal user)
        {
            return user.IsAdmin() || user.IsHr();
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("admin");
        }

        public static bool IsHr(this ClaimsPrincipal user)
        {
            return user.IsInRole("hr");
        }

        public static bool IsHighLevelSupervisor(this ClaimsPrincipal user)
        {
            return user.IsSupervisor() && user.HasClaim(claim =>
                       claim.Type == AuthenticateController.ClaimSupervisorType &&
                       claim.Value == GroupType.Supervisor.ToString());
        }

        public static bool IsSupervisor(this ClaimsPrincipal user)
        {
            return user.HasClaim(claim => claim.Type == AuthenticateController.ClaimSupervisor);
        }

        public static Guid? SupervisorGroupId(this ClaimsPrincipal user)
        {
            return user.ClaimValueAsGuid(AuthenticateController.ClaimSupervisor);
        }

        public static Guid? LeaveDelegateGroupId(this ClaimsPrincipal user)
        {
            return user.ClaimValueAsGuid(AuthenticateController.ClaimLeaveDelegate);
        }

        public static bool IsLeaveDelegate(this ClaimsPrincipal user)
        {
            return user.HasClaim(claim => claim.Type == AuthenticateController.ClaimLeaveDelegate);
        }

        private static Guid? ClaimValueAsGuid(this ClaimsPrincipal user, string claimType)
        {
            return Guid.TryParse(user.FindFirstValue(claimType), out var guid) ? guid : (Guid?) null;
        }

        static readonly MediaTypeHeaderValue JsonMediaType = MediaTypeHeaderValue.Parse("application/json");

        public static bool IsJsonRequest(this HttpContext context)
        {
            return context.Request.GetTypedHeaders().Accept?.Any(value => value.IsSubsetOf(JsonMediaType)) == true;
        }

        public static IList<T> RemoveSalary<T>(this IList<T> list) where T : Staff
        {
            foreach (var ent in list)
            {
                ent.RemoveSalary();
            }

            return list;
        }

        public static IList<T> RemoveSalaryStaff<T>(this IList<T> list) where T : PersonWithStaff
        {
            foreach (var ent in list)
            {
                ent.Staff?.RemoveSalary();
            }

            return list;
        }
    }
}