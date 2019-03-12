using System;
using System.Threading.Tasks;
using Backend.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Controllers
{
    public abstract class MyController : Controller
    {
        private readonly Lazy<IAuthorizationService> _lazyAuthService;
        protected IAuthorizationService AuthorizationService => _lazyAuthService.Value;

        protected MyController()
        {
            _lazyAuthService =
                new Lazy<IAuthorizationService>(() => HttpContext.RequestServices.GetService<IAuthorizationService>());
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        protected async Task<ActionResult<T>> TryExecute<T, TR>(MyPolicies policy, TR resource, Func<T> onSuccess)
        {
            var authorizationResult = await AuthorizationService.AuthorizeAsync(User, resource, policy.ToString());
            if (authorizationResult.Succeeded)
            {
                return onSuccess();
            }

            return new ForbidResult();
        }

        protected async Task<ActionResult<T>> TryExecute<T, TR>(MyPolicies policy, TR resource, Func<Task<T>> onSuccess)
        {
            var authorizationResult = await AuthorizationService.AuthorizeAsync(User, resource, policy.ToString());
            if (authorizationResult.Succeeded)
            {
                return await onSuccess();
            }

            return new ForbidResult();
        }

        protected async Task<ActionResult> TryExecute<TR>(MyPolicies policy,
            TR resource,
            Func<Task<ActionResult>> onSuccess)
        {
            var authorizationResult = await AuthorizationService.AuthorizeAsync(User, resource, policy.ToString());
            if (authorizationResult.Succeeded)
            {
                return await onSuccess();
            }

            return new ForbidResult();
        }
    }
}