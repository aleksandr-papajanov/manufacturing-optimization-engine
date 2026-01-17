using Common.Models;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Context flowing through the workflow optimization pipeline.
/// </summary>
public class WorkflowContext
{
    public required MotorRequest Request { get; init; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish"
    /// </summary>
    public string? WorkflowType { get; set; }
    
    /// <summary>
    /// Sequential manufacturing process steps.
    /// Upgrade: 8 steps, Refurbish: 5 steps
    /// </summary>
    public List<ProcessStep> ProcessSteps { get; set; } = new();
    
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Optimization results.
    /// </summary>
    public OptimizationResult? OptimizationResult { get; set; }
    
    public bool IsSuccess => Errors.Count == 0;
}
