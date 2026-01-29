namespace ManufacturingOptimization.Gateway.Extensions;

public static class CorsConfiguration
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                      {
                          var uri = new Uri(origin);
                          return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                      })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
