using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    public class GlobalExceptionHandler : IExceptionFilter, IAsyncExceptionFilter
    {
        private readonly ILogger _logger;

        public GlobalExceptionHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GlobalExceptionHandler>();
        }

        public void OnException(ExceptionContext context)
        {
            if (context.HttpContext.IsJsonRequest())
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
                        ControllerExtensions.RedirectFrontendMessagePath("Error: " + context.Exception.Message));
            }

            string userName = context.HttpContext.User.Identity.Name ?? "anonymous";
            _logger.LogError(0, context.Exception, "Request to {0} by {1}", context.HttpContext.Request.Path, userName);
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }
    }

    public class UserError : Exception
    {
        public UserError()
        {
        }

        public UserError(string message) : base(message)
        {
        }

        public UserError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UserError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}