using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Configurations;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.ProviderRegistry.Data;

/// <summary>
/// Database context for provider registry.
/// </summary>
public class ProviderSimulatorDbContext : DbContext, IProviderSimulatorDbContext
{
    public DbSet<PlannedProcessEntity> PlannedProcesses => Set<PlannedProcessEntity>();
    public DbSet<ProposalEntity> Proposals => Set<ProposalEntity>();
    public DbSet<ProcessEstimateEntity> ProcessEstimates => Set<ProcessEstimateEntity>();

    public ProviderSimulatorDbContext(DbContextOptions<ProviderSimulatorDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PlannedProcessConfiguration());
        modelBuilder.ApplyConfiguration(new ProposalConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessEstimateConfiguration());
    }
}

public class ProviderSimulatorDbContextFactory : IDesignTimeDbContextFactory<ProviderSimulatorDbContext>
{
    public ProviderSimulatorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProviderSimulatorDbContext>();
        
        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "provider-simulator.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new ProviderSimulatorDbContext(optionsBuilder.Options);
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "provider-simulator.db");

        services.AddDbContext<ProviderSimulatorDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IProviderSimulatorDbContext, ProviderSimulatorDbContext>();

        return services;
    }
}