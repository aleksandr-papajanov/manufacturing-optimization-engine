using ManufacturingOptimization.Engine.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// Background service for managing database lifecycle.
/// Clears providers on startup and shutdown since they re-register on every start.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = true;

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
            var dbContext = scope.ServiceProvider.GetRequiredService<EngineDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                await RecreateDatabase(dbContext, cancellationToken);
            }
            else
            {
                // Ensure database is created
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }

            ClearProvidersAsync(dbContext, cancellationToken).Wait(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Engine database");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EngineDbContext>();

            ClearProvidersAsync(dbContext, cancellationToken).Wait(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup Engine database");
        }
    }

    private async Task ClearProvidersAsync(EngineDbContext dbContext, CancellationToken cancellationToken)
    {
        var providersCount = await dbContext.Providers.CountAsync(cancellationToken);
        if (providersCount > 0)
        {
            _logger.LogInformation("Clearing {Count} providers from database", providersCount);
            dbContext.Providers.RemoveRange(dbContext.Providers);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RecreateDatabase(EngineDbContext dbContext, CancellationToken cancellationToken)
    {
        // Delete existing database
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        
        // Create fresh database with current schema
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
