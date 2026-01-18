namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Represents one manufacturing process step (e.g., Cleaning, Redesign, Turning).
/// </summary>
public class WorkflowProcessStep
{
    public int StepNumber { get; set; }
    public required string Activity { get; init; }
    public required string RequiredCapability { get; init; }
    public string? Description { get; set; }
    
    /// <summary>
    /// Providers that can perform this step.
    /// </summary>
    public List<MatchedProvider> MatchedProviders { get; set; } = new();
    
    /// <summary>
    /// Selected provider after optimization.
    /// </summary>
    public MatchedProvider? SelectedProvider { get; set; }
}
