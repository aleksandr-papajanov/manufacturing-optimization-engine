using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IOptimizationStrategyRepository : IRepository<OptimizationStrategyEntity>
{
    void StoreStrategies(Guid requestId, List<OptimizationStrategyEntity> strategies);
    List<OptimizationStrategyEntity>? GetStrategies(Guid requestId);
    void RemoveStrategies(Guid requestId);
}
