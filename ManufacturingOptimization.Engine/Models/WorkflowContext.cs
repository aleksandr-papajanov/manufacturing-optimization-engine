using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Context flowing through the workflow optimization pipeline.
/// Works with Entity classes internally.
/// </summary>
public class WorkflowContext
{
    public required OptimizationRequestModel Request { get; init; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish"
    /// </summary>
    public string? WorkflowType { get; set; }
    
    /// <summary>
    /// Sequential manufacturing process steps.
    /// Upgrade: 8 steps, Refurbish: 5 steps
    /// </summary>
    public List<WorkflowProcessStep> ProcessSteps { get; set; } = [];
    
    /// <summary>
    /// Optimization results (used for backward compatibility with single optimization).
    /// </summary>
    public OptimizationMetricsModel? OptimizationMetrics { get; set; }
    
    /// <summary>
    /// Generated optimization strategies with different priorities.
    /// Each strategy represents a different way to optimize the workflow.
    /// </summary>
    public List<OptimizationStrategyModel> Strategies { get; set; } = [];
    
    /// <summary>
    /// Strategy selected by the customer.
    /// </summary>
    public OptimizationStrategyModel? SelectedStrategy { get; set; }
    
    /// <summary>
    /// Plan ID assigned after strategy selection.
    /// Used for provider confirmations before actual plan persistence.
    /// </summary>
    public Guid? PlanId { get; set; }
    
    /// <summary>
    /// Saved optimization plan (available after PlanPersistenceStep).
    /// </summary>
    public OptimizationPlanEntity? SavedPlan { get; set; }
}

