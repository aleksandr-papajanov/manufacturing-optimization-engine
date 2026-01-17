using System.Collections.Concurrent;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services;

public class InMemoryProviderRepository : IProviderRepository
{
    private readonly ConcurrentDictionary<Guid, RegisteredProvider> _providers = new();
    private readonly ILogger<InMemoryProviderRepository> _logger;

    public int Count => _providers.Count;

    public InMemoryProviderRepository(ILogger<InMemoryProviderRepository> logger)
    {
        _logger = logger;
    }

    public void Create(Guid providerId, string providerType, string providerName, List<string> capabilities,
        double axisHeight = 0, double power = 0, double tolerance = 0)
    {
        var provider = new RegisteredProvider
        {
            ProviderId = providerId,
            ProviderType = providerType,
            ProviderName = providerName,
            Capabilities = capabilities ?? new(),
            AxisHeight = axisHeight,
            Power = power,
            Tolerance = tolerance
        };

        _providers[providerId] = provider;
        
        _logger.LogInformation("Registered provider {ProviderId} ({Name}) - Capabilities: {Capabilities}, Power: {Power}kW, AxisHeight: {AxisHeight}mm",
            providerId, providerName, string.Join(", ", provider.Capabilities), power, axisHeight);
    }

    public List<RegisteredProvider> GetAll()
    {
        return _providers.Values.ToList();
    }
    
    public List<RegisteredProvider> FindByCapability(string capability)
    {
        return _providers.Values
            .Where(p => p.Capabilities.Contains(capability, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }
}
