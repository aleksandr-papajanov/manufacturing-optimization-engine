using Common.Models;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IOptimizationStrategyRepository
{
    void StoreStrategies(Guid requestId, List<OptimizationStrategy> strategies);
    List<OptimizationStrategy>? GetStrategies(Guid requestId);
    void RemoveStrategies(Guid requestId);
}
