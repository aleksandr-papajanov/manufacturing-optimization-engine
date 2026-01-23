using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.ProviderRegistry.Data;

/// <summary>
/// Design-time factory for creating ProviderRegistryDbContext during migrations.
/// This allows EF Core tools to create the DbContext without starting the full application.
/// </summary>
public class ProviderRegistryDbContextFactory : IDesignTimeDbContextFactory<ProviderRegistryDbContext>
{
    public ProviderRegistryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProviderRegistryDbContext>();
        
        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "providers.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new ProviderRegistryDbContext(optionsBuilder.Options);
    }
}
