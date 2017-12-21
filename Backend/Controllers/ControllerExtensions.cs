using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
            return controller.Redirect(RedirectFrontendPath(message));
        }

        public static string RedirectFrontendPath(string message)
        {
            return $"~/message?text={Uri.EscapeDataString(message)}";
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
    }
}