using System.Collections.Concurrent;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services;

public class InMemoryProviderRepository : IProviderRepository
{
    private readonly ConcurrentDictionary<Guid, Provider> _providers = new();
    private readonly ILogger<InMemoryProviderRepository> _logger;

    public int Count => _providers.Count;

    public InMemoryProviderRepository(ILogger<InMemoryProviderRepository> logger)
    {
        _logger = logger;
    }

    public void Create(Provider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        _providers[provider.Id] = provider;
        
        _logger.LogInformation("Registered provider {ProviderId} ({Name}) - Processes: {ProcessCount}, Power: {Power}kW, AxisHeight: {AxisHeight}mm",
            provider.Id, provider.Name, provider.ProcessCapabilities.Count, 
            provider.TechnicalCapabilities.Power, provider.TechnicalCapabilities.AxisHeight);
    }

    public List<Provider> GetAll()
    {
        return _providers.Values.ToList();
    }
    
    public List<(Provider Provider, ProviderProcessCapability Capability)> FindByProcess(string processName)
    {
        return _providers.Values
            .SelectMany(p => p.ProcessCapabilities
                .Where(cap => cap.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                .Select(cap => (Provider: p, Capability: cap)))
            .ToList();
    }
}
