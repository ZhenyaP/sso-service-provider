using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using ServiceProvider.API.Entities;

namespace ServiceProvider.API.Middlewares
{
    public class ErrorContextInitializationMiddleware
    {
        /// <summary>
        /// The next middleware.
        /// </summary>
        private readonly RequestDelegate _next;

        public ErrorContextInitializationMiddleware(RequestDelegate next,
            IHostApplicationLifetime lifetime)
        {
            _next = next;
            lifetime.ApplicationStarted.Register(() => ErrorContext.CreateNewErrorContext());
        }

        /// <summary>
        /// The middleware execution logic.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>The Task object.</returns>
        public async Task Invoke(HttpContext context)
        {
            // Pass the request to the next middleware
            await _next(context);
        }
    }
}
