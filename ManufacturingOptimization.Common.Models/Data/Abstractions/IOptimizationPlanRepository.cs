using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

/// <summary>
/// Repository for storing and retrieving optimization plans.
/// </summary>
public interface IOptimizationPlanRepository : IRepository<OptimizationPlanEntity>
{
    /// <summary>
    /// Get plan by request ID (since customer uses RequestId for tracking)
    /// </summary>
    Task<OptimizationPlanEntity?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
