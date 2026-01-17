namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Provider matched to a specific process step.
/// </summary>
public class MatchedProvider
{
    public required Guid ProviderId { get; init; }
    public required string ProviderName { get; init; }
    public required string ProviderType { get; init; }
    
    // Optimization metrics
    public decimal CostEstimate { get; set; }
    public TimeSpan TimeEstimate { get; set; }
    public double QualityScore { get; set; } // 0.0 - 1.0
    public double EmissionsKgCO2 { get; set; } // Carbon emissions
}
