using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Context flowing through the workflow optimization pipeline.
/// </summary>
public class WorkflowContext
{
    public required OptimizationRequest Request { get; init; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish"
    /// </summary>
    public string? WorkflowType { get; set; }
    
    /// <summary>
    /// Sequential manufacturing process steps.
    /// Upgrade: 8 steps, Refurbish: 5 steps
    /// </summary>
    public List<WorkflowProcessStep> ProcessSteps { get; set; } = [];
    
    public List<string> Errors { get; set; } = [];
    
    /// <summary>
    /// Optimization results (used for backward compatibility with single optimization).
    /// </summary>
    public OptimizationMetrics? OptimizationMetrics { get; set; }
    
    /// <summary>
    /// Generated optimization strategies with different priorities.
    /// Each strategy represents a different way to optimize the workflow.
    /// </summary>
    public List<OptimizationStrategy> Strategies { get; set; } = [];
    
    /// <summary>
    /// Strategy selected by the customer.
    /// </summary>
    public OptimizationStrategy? SelectedStrategy { get; set; }
    
    public bool IsSuccess => Errors.Count == 0;
}

