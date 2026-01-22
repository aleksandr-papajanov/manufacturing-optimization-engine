using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Provider matched to a specific process step.
/// </summary>
public class MatchedProvider
{
    public required Guid ProviderId { get; init; }
    
    /// <summary>
    /// Provider name for display.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider's estimate for this process (cost, time, quality, emissions).
    /// </summary>
    public ProcessEstimateModel Estimate { get; set; } = new();
}
