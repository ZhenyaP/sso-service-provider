using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ServiceProvider.API.Entities;
using ILogger = Serilog.ILogger;

namespace ServiceProvider.API.Middlewares
{
    /// <summary>
    /// The Request Logger Middleware.
    /// </summary>
    public class RequestLoggerMiddleware
    {
        /// <summary>
        /// The message template
        /// </summary>
        public const string MessageTemplate = @"HTTP {RequestMethod} {RequestPath} responded 
{StatusCode} in {Elapsed:0.0000} ms. 
RequestHeaders = {RequestHeaders}; 
RequestHost = {RequestHost}; 
RequestProtocol = {RequestProtocol}; 
RequestForm = {RequestForm}";

        /// <summary>
        /// The next middleware
        /// </summary>
        private readonly RequestDelegate _next;

        private readonly ILogger<RequestLoggerMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware.</param>
        /// <param name="logger">Logger</param>
        public RequestLoggerMiddleware(RequestDelegate next, 
            ILogger<RequestLoggerMiddleware> logger)
        {
            this._next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger;
        }

        /// <summary>
        /// Invokes the Request Logger Middleware.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">httpContext</exception>
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            using (await Timer.SetCurrentTimerAsync(httpContext.Request.Path.Value))
            {
                try
                {
                    await this._next(httpContext);

                    var statusCode = httpContext.Response?.StatusCode;
                    var level = statusCode > 499 ? LogEventLevel.Error : LogEventLevel.Information;
                    LogForContext(httpContext).Write(level, MessageTemplate);
                }
                catch (Exception ex)
                {
                    foreach (var message in ErrorContext.Current.GetMessages())
                    {
                        _logger.LogError(message);
                    }
                    LogForContext(httpContext).Error(ex, MessageTemplate);
                }
            }

        }

        /// <summary>
        /// Logs for HTTP context.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>The Logger.</returns>
        private static ILogger LogForContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var result = Log.ForContext("RequestMethod", request.Method)
                .ForContext("RequestPath", request.Path)
                .ForContext("StatusCode", httpContext.Response?.StatusCode)
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()), destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
            {
                result = result.ForContext(
                    "RequestForm",
                    request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
            }

            return result;
        }
    }
}
