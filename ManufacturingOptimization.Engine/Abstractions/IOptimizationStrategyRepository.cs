using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Repository for managing optimization strategy entities.
/// </summary>
public interface IOptimizationStrategyRepository : IRepository<OptimizationStrategyEntity>
{
    /// <summary>
    /// Get strategy by its unique StrategyId (Guid).
    /// </summary>
    Task<OptimizationStrategyEntity?> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all strategies for a specific request.
    /// </summary>
    Task<IEnumerable<OptimizationStrategyEntity>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
