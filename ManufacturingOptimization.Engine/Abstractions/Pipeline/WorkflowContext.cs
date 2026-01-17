using Common.Models;

namespace ManufacturingOptimization.Engine.Abstractions.Pipeline;

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
    
    public bool IsSuccess => Errors.Count == 0;
}

/// <summary>
/// Represents one manufacturing process step (e.g., Cleaning, Redesign, Turning).
/// </summary>
public class ProcessStep
{
    public int StepNumber { get; set; }
    public required string Activity { get; init; }
    public required string RequiredCapability { get; init; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Providers that can perform this step.
    /// </summary>
    public List<MatchedProvider> MatchedProviders { get; set; } = new();
}

/// <summary>
/// Provider matched to a specific process step.
/// </summary>
public class MatchedProvider
{
    public required Guid ProviderId { get; init; }
    public required string ProviderName { get; init; }
    public required string ProviderType { get; init; }
}
