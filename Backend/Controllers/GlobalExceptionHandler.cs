using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Backend.Entities;
using LinqToDB.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Sentinel.Sdk;
using Sentinel.Sdk.Abstractions;
using Sentinel.Sdk.Enums;
using Sentinel.Sdk.Middleware;

namespace Backend.Controllers
{
    public class GlobalExceptionHandler : IExceptionFilter, IAsyncExceptionFilter
    {
        private readonly ISentinelClientFactory _sentinelClientFactory;
        private readonly SentinelSettings _sentinelSettings;
        private readonly ILogger _logger;

        public GlobalExceptionHandler(ILoggerFactory loggerFactory,
            ISentinelClientFactory sentinelClientFactory,
            IOptions<SentinelSettings> sentinelSettings)
        {
            _sentinelClientFactory = sentinelClientFactory;
            _sentinelSettings = sentinelSettings.Value;
            _logger = loggerFactory.CreateLogger<GlobalExceptionHandler>();
        }

        public void OnException(ExceptionContext context)
        {
            ReportToSentry(context);

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

        private void ReportToSentry(ExceptionContext context)
        {
            if (!_sentinelSettings.Environment.Equals("production", StringComparison.InvariantCultureIgnoreCase)) return;
            var sentryClient = _sentinelClientFactory.CreateClient(_sentinelSettings, context.HttpContext);
            //we're using user and http the right way, not via contexts
            sentryClient.Contexts.Remove("user");
            sentryClient.Contexts.Remove("http");
            try
            {
                sentryClient.Capture(CreateEvent(context));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when reporting an error to sentry");
            }
        }

        private SentryEvent CreateEvent(ExceptionContext context)
        {
            var httpContext = context.HttpContext;
            MySentryEvent sentryEvent = new MySentryEvent(context.Exception)
            {
                User = new UserSentryContext(httpContext),
                Request = new HttpSentryContext(httpContext) {Cookies = null}
            };
            sentryEvent.Logger = ".NET";
            sentryEvent.Tags.Add("logger", ".NET");
            sentryEvent.Request.Headers.Remove("Cookie");
            if (sentryEvent.User.UserName.IsNullOrEmpty())
            {
                sentryEvent.User.UserName = "anonymous";
            }

            return sentryEvent;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }
    }

    public class MySentryEvent : SentryEvent
    {
        public MySentryEvent(Exception exception, SeverityLevel level = SeverityLevel.Error) : base(exception, level)
        {
            Tags = new Dictionary<string, string>();
        }

        public MySentryEvent(string message, SeverityLevel level = SeverityLevel.Info) : base(message, level)
        {
        }

        public HttpSentryContext Request { get; set; }
        public UserSentryContext User { get; set; }
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