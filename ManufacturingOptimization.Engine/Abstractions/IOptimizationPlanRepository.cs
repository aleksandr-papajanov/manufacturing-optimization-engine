using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Repository interface for managing optimization plans.
/// </summary>
public interface IOptimizationPlanRepository : IRepository<OptimizationPlanEntity>
{
    /// <summary>
    /// Get an optimization plan by the original request ID.
    /// </summary>
    /// <param name="requestId">The customer request identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The plan if found, null otherwise.</returns>
    Task<OptimizationPlanEntity?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
}
