using ManufacturingOptimization.Common.Messaging.Abstractions;
using System.Text.Json;

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
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "application/json";

                var errorResponse = new
                {
                    Status = "Service Unavailable",
                    Message = "System is still initializing. Please try again in a few moments.",
                    Timestamp = DateTime.UtcNow
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }));
                return;
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
