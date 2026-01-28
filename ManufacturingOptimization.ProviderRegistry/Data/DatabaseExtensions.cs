using ManufacturingOptimization.Common.Models.Data.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ManufacturingOptimization.ProviderRegistry.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "providers.db");

        services.AddDbContext<ProviderRegistryDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        
        services.AddScoped<IProviderDbContext, ProviderRegistryDbContext>();

        return services;
    }
}
