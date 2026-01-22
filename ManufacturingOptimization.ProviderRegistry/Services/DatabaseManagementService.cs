using AutoMapper;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Background service that initializes the database with provider data on startup.
/// Reads from providers.json and seeds the database if empty.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = true;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;
    private readonly IMapper _mapper;
    private readonly string _seedDataPath;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger,
        IMapper mapper)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapper = mapper;
        _seedDataPath = Path.Combine(AppContext.BaseDirectory, "providers.json");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProviderRegistryDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                await RecreateDatabase(dbContext, cancellationToken);
            }
            else
            {
                // Ensure database is created
                await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            }

            // Check if database already has data
            var existingProvidersCount = await dbContext.Providers.CountAsync(cancellationToken);
            if (existingProvidersCount > 0)
            {
                return;
            }

            // Load seed data from JSON
            if (!File.Exists(_seedDataPath))
            {
                return;
            }

            var json = await File.ReadAllTextAsync(_seedDataPath, cancellationToken);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<ProvidersConfig>(json, options);

            if (config?.Providers == null || config.Providers.Length == 0)
            {
                return;
            }

            // Map providers to entities and add to database
            var providerEntities = _mapper.Map<List<ProviderEntity>>(config.Providers);
            await dbContext.Providers.AddRangeAsync(providerEntities, cancellationToken);
            var saved = await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task RecreateDatabase(ProviderRegistryDbContext dbContext, CancellationToken cancellationToken)
    {
        // Delete existing database
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        
        // Create fresh database with current schema
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}

/// <summary>
/// Configuration wrapper for deserializing providers.json.
/// </summary>
internal class ProvidersConfig
{
    public Provider[] Providers { get; set; } = Array.Empty<Provider>();
}
