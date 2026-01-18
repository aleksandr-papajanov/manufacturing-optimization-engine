namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Manufacturing plan created after customer selects their preferred strategy.
/// Contains the selected strategy and execution status information.
/// </summary>
public class OptimizationPlan
{
    /// <summary>
    /// Unique identifier for this plan.
    /// </summary>
    public Guid PlanId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Original customer request ID.
    /// </summary>
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// The strategy selected by the customer.
    /// Contains all workflow details (steps, costs, warranty, etc.).
    /// </summary>
    public OptimizationStrategy? SelectedStrategy { get; set; }
    
    /// <summary>
    /// Current status of the plan execution.
    /// </summary>
    public OptimizationPlanStatus Status { get; set; } = OptimizationPlanStatus.Draft;
    
    /// <summary>
    /// Timestamp when the plan was created (strategies generated).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when the customer selected a strategy.
    /// </summary>
    public DateTime? SelectedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the plan was confirmed and sent to providers.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
    
    /// <summary>
    /// Indicates whether the plan creation was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// List of errors encountered during plan creation.
    /// </summary>
    public List<string> Errors { get; set; } = [];
}

