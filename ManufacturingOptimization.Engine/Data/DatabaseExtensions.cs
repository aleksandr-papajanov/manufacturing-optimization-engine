using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Engine.Data;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "engine.db");

        services.AddDbContext<EngineDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        
        services.AddScoped<IProviderDbContext, EngineDbContext>();
        services.AddScoped<IOptimizationDbContext, EngineDbContext>();

        return services;
    }
}
