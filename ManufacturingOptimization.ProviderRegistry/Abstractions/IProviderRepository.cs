using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.ProviderRegistry.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
}