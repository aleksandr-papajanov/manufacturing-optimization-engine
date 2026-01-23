using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.Engine.Data;

/// <summary>
/// Design-time factory for creating EngineDbContext during migrations.
/// This allows EF Core tools to create the DbContext without starting the full application.
/// </summary>
public class EngineDbContextFactory : IDesignTimeDbContextFactory<EngineDbContext>
{
    public EngineDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EngineDbContext>();
        
        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "engine.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new EngineDbContext(optionsBuilder.Options);
    }
}
