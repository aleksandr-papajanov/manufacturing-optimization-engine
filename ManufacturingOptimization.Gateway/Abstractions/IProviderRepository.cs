using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IProviderRepository
{
    void Create(Provider provider);
    List<Provider> GetAll();
    Provider? GetById(Guid providerId);
}
