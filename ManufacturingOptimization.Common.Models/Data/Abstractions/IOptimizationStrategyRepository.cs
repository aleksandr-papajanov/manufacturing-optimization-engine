using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

/// <summary>
/// Repository for managing optimization strategy entities.
/// </summary>
public interface IOptimizationStrategyRepository : IRepository<OptimizationStrategyEntity>
{
    Task AddForRequestAsync(Guid requestId, List<OptimizationStrategyEntity> strategies);
    Task<List<OptimizationStrategyEntity>?> GetForRequestAsync(Guid requestId);
    Task RemoveForRequestAsync(Guid requestId);
}
