namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Single step in optimized workflow with selected provider.
/// </summary>
public class OptimizationProcessStep
{
    public int StepNumber { get; set; }
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// Selected provider for this step.
    /// </summary>
    public Guid SelectedProviderId { get; set; }
    
    /// <summary>
    /// Selected provider name for display.
    /// </summary>
    public string SelectedProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Process estimate from selected provider.
    /// </summary>
    public ProcessEstimate Estimate { get; set; } = new();
}
