using System.Linq;
using Backend.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Backend.Controllers
{
    public class GlobalExceptionHandler : IExceptionFilter
    {
        private readonly ILogger _logger;

        public GlobalExceptionHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GlobalExceptionHandler>();
        }

        public void OnException(ExceptionContext context)
        {
            var jsonMediaType = MediaTypeHeaderValue.Parse("application/json");
            if (context.HttpContext.Request.GetTypedHeaders().Accept.Any(value => value.IsSubsetOf(jsonMediaType)))
            {
                context.Result = new ObjectResult(new ErrorResponse {Message = context.Exception.Message})
                {
                    StatusCode = 500,
                    DeclaredType = typeof(ErrorResponse)
                };
            }
            else
            {
                context.Result =
                    new RedirectResult(
                        ControllerExtensions.RedirectFrontendPath("Error: " + context.Exception.Message));
            }

            string userName = context.HttpContext.User.Identity.Name ?? "anonymous";
            _logger.LogError(0, context.Exception, "Request to {0} by {1}", context.HttpContext.Request.Path, userName);
        }
    }
}