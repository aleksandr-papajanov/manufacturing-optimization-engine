using System.Collections.Concurrent;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services;

public class InMemoryProviderRepository : IProviderRepository
{
    private readonly ConcurrentDictionary<Guid, RegisteredProvider> _providers = new();
    private readonly ILogger<InMemoryProviderRepository> _logger;

    public InMemoryProviderRepository(ILogger<InMemoryProviderRepository> logger)
    {
        _logger = logger;
    }

    public void Create(Guid providerId, string providerType, string providerName)
    {
        var provider = new RegisteredProvider
        {
            ProviderId = providerId,
            ProviderType = providerType,
            ProviderName = providerName
        };

        _providers[providerId] = provider;
    }

    public List<RegisteredProvider> GetAll()
    {
        return _providers.Values.ToList();
    }

    public int Count => _providers.Count;
}
