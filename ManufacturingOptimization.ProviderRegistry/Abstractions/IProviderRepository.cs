using ManufacturingOptimization.ProviderRegistry.Entities;

namespace ManufacturingOptimization.ProviderRegistry.Abstractions;

public interface IProviderRepository
{
    Task<IEnumerable<Provider>> GetAllAsync(CancellationToken cancellationToken = default);
}