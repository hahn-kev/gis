using System;
using System.Linq;
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
    public class GlobalExceptionHandler : IExceptionFilter
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

        private void ReportToSentry(ExceptionContext context)
        {
            var sentryClient = _sentinelClientFactory.CreateClient(_sentinelSettings, context.HttpContext);
            //we're using user and http the right way, not via contexts
            sentryClient.Contexts.Remove("user");
            sentryClient.Contexts.Remove("http");
            sentryClient.Capture(CreateEvent(context));
        }

        private SentryEvent CreateEvent(ExceptionContext context)
        {
            var httpContext = context.HttpContext;
            MySentryEvent sentryEvent = new MySentryEvent(context.Exception)
            {
                User = new UserSentryContext(httpContext),
                Request = new HttpSentryContext(httpContext)
            };
            if (sentryEvent.User.Username.IsNullOrEmpty())
            {
                sentryEvent.User.Username = "anonymous";
            }
            return sentryEvent;
        }
    }

    public class MySentryEvent : SentryEvent
    {
        public MySentryEvent(Exception exception, SeverityLevel level = SeverityLevel.Error) : base(exception, level)
        {
        }

        public MySentryEvent(string message, SeverityLevel level = SeverityLevel.Info) : base(message, level)
        {
        }

        public HttpSentryContext Request { get; set; }
        public UserSentryContext User { get; set; }
    }
}