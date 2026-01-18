using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.ProviderRegistry.Abstractions;

public interface IProviderRepository
{
    Task<IEnumerable<Provider>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken = default);
}