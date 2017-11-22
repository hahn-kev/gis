using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public static class ControllerExtensions
    {
        public static IActionResult Errors(this IdentityResult result)
        {
            throw new ArgumentException(string.Join(Environment.NewLine, result.Errors
                .Select(x => x.Description)));
            return new BadRequestResult();
        }

        public static IActionResult ShowFrontendMessage(this Controller controller, string message)
        {
            return controller.Redirect(RedirectFrontendPath(message));
        }

        public static string RedirectFrontendPath(string message)
        {
            return $"~/message?text={Uri.EscapeDataString(message)}";
        }
    }
}