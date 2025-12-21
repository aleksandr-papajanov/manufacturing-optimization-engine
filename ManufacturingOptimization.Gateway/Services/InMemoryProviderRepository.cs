using System.Collections.Concurrent;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

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
            ProviderName = providerName,
            RegisteredAt = DateTime.UtcNow
        };

        _providers[providerId] = provider;
    }

    public List<RegisteredProvider> GetAll()
    {
        return _providers.Values.OrderBy(p => p.RegisteredAt).ToList();
    }

    public RegisteredProvider? GetById(Guid providerId)
    {
        return _providers.TryGetValue(providerId, out var provider) ? provider : null;
    }
}
