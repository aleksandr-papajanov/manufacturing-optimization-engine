using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Gateway.Abstractions;

/// <summary>
/// Repository for storing and retrieving optimization plans.
/// </summary>
public interface IOptimizationPlanRepository : IRepository<OptimizationPlan, Guid>
{
    /// <summary>
    /// Get plan by request ID (since customer uses RequestId for tracking)
    /// </summary>
    OptimizationPlan? GetByRequestId(Guid requestId);
}
