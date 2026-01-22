using ManufacturingOptimization.Gateway.Data;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Services;

/// <summary>
/// Background service for managing database lifecycle.
/// Clears providers on startup and shutdown since they re-register on every start.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = false;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                // Clear for development purposes
                await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            // Apply migrations
            await dbContext.Database.MigrateAsync(cancellationToken);

            ClearProvidersAsync(dbContext, cancellationToken).Wait(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Gateway database");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

            ClearProvidersAsync(dbContext, cancellationToken).Wait(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup Gateway database");
        }
    }

    private async Task ClearProvidersAsync(GatewayDbContext dbContext, CancellationToken cancellationToken)
    {
        var providersCount = await dbContext.Providers.CountAsync(cancellationToken);
        if (providersCount > 0)
        {
            _logger.LogInformation("Clearing {Count} providers from database", providersCount);
            dbContext.Providers.RemoveRange(dbContext.Providers);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RecreateDatabase(GatewayDbContext dbContext, CancellationToken cancellationToken)
    {
        // Delete existing database
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        
        // Create fresh database with current schema
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
