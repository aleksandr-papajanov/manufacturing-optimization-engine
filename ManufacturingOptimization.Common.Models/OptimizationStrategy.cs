namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Represents a single optimization strategy option for the customer.
/// Each strategy optimizes for a different priority (Cost, Time, Quality, Emissions).
/// Contains all workflow details including steps, costs, and warranties.
/// </summary>
public class OptimizationStrategy
{
    /// <summary>
    /// Unique identifier for this strategy.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Foreign key to associated plan (null if not selected yet).
    /// </summary>
    public Guid? PlanId { get; set; }
    
    /// <summary>
    /// Original request ID for filtering (non-DB field, used during workflow).
    /// </summary>
    public Guid? RequestId { get; set; }
    
    /// <summary>
    /// Human-readable strategy name (e.g., "Budget Strategy", "Express Strategy").
    /// </summary>
    public string StrategyName { get; set; } = string.Empty;
    
    /// <summary>
    /// The optimization priority used to generate this strategy.
    /// </summary>
    public OptimizationPriority Priority { get; set; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish".
    /// </summary>
    public string WorkflowType { get; set; } = string.Empty;
    
    /// <summary>
    /// Sequential process steps with selected providers for this strategy.
    /// </summary>
    public List<OptimizationProcessStep> Steps { get; set; } = [];
    
    /// <summary>
    /// Optimization metrics (cost, time, quality, emissions, solver status).
    /// </summary>
    public OptimizationMetrics Metrics { get; set; } = new();
    
    /// <summary>
    /// Warranty terms offered with this strategy.
    /// </summary>
    public WarrantyTerms Warranty { get; set; } = new();
    
    /// <summary>
    /// Description explaining why this strategy might be chosen.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when this strategy was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

