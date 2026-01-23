using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Middleware
{
    public class SystemReadinessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SystemReadinessMiddleware> _logger;

        public SystemReadinessMiddleware(RequestDelegate next, ILogger<SystemReadinessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ISystemReadinessService readinessService)
        {
            // Check if system is ready
            if (!readinessService.IsSystemReady || !readinessService.IsProvidersReady)
            {
                throw new ServiceNotReadyException();
            }

            await _next(context);
        }
    }

    public static class SystemReadinessMiddlewareExtensions
    {
        public static IApplicationBuilder UseSystemReadiness(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SystemReadinessMiddleware>();
        }
    }
}
