using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
}
