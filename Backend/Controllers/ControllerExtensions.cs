using System;
using System.Linq;
using System.Security.Claims;
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

        public static IActionResult ShowFrontendMessage(this Controller controller, string message)
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
            return Guid.TryParse(user.FindFirstValue(AuthenticateController.ClaimPersonId), out var guid)
                ? guid
                : (Guid?) null;
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

        static readonly MediaTypeHeaderValue JsonMediaType = MediaTypeHeaderValue.Parse("application/json");

        public static bool IsJsonRequest(this HttpContext context)
        {
            return context.Request.GetTypedHeaders().Accept?.Any(value => value.IsSubsetOf(JsonMediaType)) == true;
        }
    }
}