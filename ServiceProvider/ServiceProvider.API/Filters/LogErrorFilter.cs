using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using ServiceProvider.API.Entities;

namespace ServiceProvider.API.Filters
{
    public class LogErrorFilter : IActionFilter
    {
        private readonly ILogger<LogErrorFilter> _logger;

        public LogErrorFilter(ILogger<LogErrorFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                
                _logger.LogError(context.Exception, $"Request: {context.HttpContext.Request.GetEncodedUrl()}");
            }
        }

        public void OnActionExecuting(ActionExecutingContext context) { }
    }
}
