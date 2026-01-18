using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Repository interface for managing optimization plans.
/// </summary>
public interface IOptimizationPlanRepository
{
    /// <summary>
    /// Create a new optimization plan.
    /// </summary>
    /// <param name="plan">The plan to create.</param>
    /// <returns>The created plan with generated ID.</returns>
    OptimizationPlan Create(OptimizationPlan plan);
    
    /// <summary>
    /// Get an optimization plan by its unique identifier.
    /// </summary>
    /// <param name="planId">The plan identifier.</param>
    /// <returns>The plan if found, null otherwise.</returns>
    OptimizationPlan? GetById(Guid planId);
    
    /// <summary>
    /// Get an optimization plan by the original request ID.
    /// </summary>
    /// <param name="requestId">The customer request identifier.</param>
    /// <returns>The plan if found, null otherwise.</returns>
    OptimizationPlan? GetByRequestId(Guid requestId);
    
    /// <summary>
    /// Update an existing optimization plan.
    /// </summary>
    /// <param name="plan">The plan to update.</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    bool Update(OptimizationPlan plan);
    
    /// <summary>
    /// Get all optimization plans.
    /// </summary>
    /// <returns>Collection of all plans.</returns>
    IEnumerable<OptimizationPlan> GetAll();
}
