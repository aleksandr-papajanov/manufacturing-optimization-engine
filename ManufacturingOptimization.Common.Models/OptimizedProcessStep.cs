namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Single step in optimized workflow with selected provider.
/// </summary>
public class OptimizedProcessStep
{
    public int StepNumber { get; set; }
    public string Activity { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected provider for this step.
    /// </summary>
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Cost estimate from selected provider.
    /// </summary>
    public decimal CostEstimate { get; set; }
    
    /// <summary>
    /// Time estimate from selected provider.
    /// </summary>
    public TimeSpan TimeEstimate { get; set; }
    
    /// <summary>
    /// Quality score from selected provider (0.0 - 1.0).
    /// </summary>
    public double QualityScore { get; set; }
    
    /// <summary>
    /// Estimated carbon emissions in kg CO2 for this step.
    /// </summary>
    public double EmissionsKgCO2 { get; set; }
}
