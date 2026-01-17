using System.Collections.Concurrent;
using Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class InMemoryProviderRepository : IProviderRepository
{
    private readonly ConcurrentDictionary<Guid, Provider> _providers = new();
    private readonly ILogger<InMemoryProviderRepository> _logger;

    public InMemoryProviderRepository(ILogger<InMemoryProviderRepository> logger)
    {
        _logger = logger;
    }

    public void Create(Provider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));
            
        _providers[provider.Id] = provider;
    }

    public List<Provider> GetAll()
    {
        return _providers.Values.OrderBy(p => p.Name).ToList();
    }

    public Provider? GetById(Guid providerId)
    {
        return _providers.TryGetValue(providerId, out var provider) ? provider : null;
    }
}
