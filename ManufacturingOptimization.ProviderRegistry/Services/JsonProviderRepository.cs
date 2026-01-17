using ManufacturingOptimization.ProviderRegistry.Abstractions;
using Common.Models;
using System.Text.Json;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Repository for provider definitions.
/// Currently reads from providers.json, can be replaced with DB implementation later.
/// </summary>
public class JsonProviderRepository : IProviderRepository
{
    private readonly ILogger<JsonProviderRepository> _logger;
    private readonly string _configPath;

    public JsonProviderRepository(ILogger<JsonProviderRepository> logger)
    {
        _logger = logger;
        _configPath = Path.Combine(AppContext.BaseDirectory, "providers.json");
    }

    public async Task<IEnumerable<Provider>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return Array.Empty<Provider>();
            }

            var json = await File.ReadAllTextAsync(_configPath, cancellationToken);
            
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = JsonSerializer.Deserialize<ProvidersConfig>(json, options);
            
            var providers = config?.Providers ?? Array.Empty<Provider>();
            
            return providers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load providers from {Path}", _configPath);
            throw;
        }
    }

    public async Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var providers = await GetAllAsync(cancellationToken);
        return providers.FirstOrDefault(p => p.Id == providerId);
    }

    private class ProvidersConfig
    {
        public Provider[] Providers { get; set; } = Array.Empty<Provider>();
    }
}
