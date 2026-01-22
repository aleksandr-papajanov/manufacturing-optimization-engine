namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Represents a provider's estimate for executing a specific process.
/// Contains cost, time, quality, and environmental impact metrics.
/// </summary>
public class ProcessEstimate
{
    /// <summary>
    /// Unique identifier for this estimate.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Estimated cost in currency.
    /// </summary>
    public decimal Cost { get; set; }
    
    /// <summary>
    /// Estimated time to complete the process.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Quality score (0.0 - 1.0).
    /// Higher values indicate better quality.
    /// </summary>
    public double QualityScore { get; set; }
    
    /// <summary>
    /// Estimated carbon emissions in kg CO2.
    /// Calculated as: EnergyConsumption (kWh) × CarbonIntensity (kg CO2/kWh).
    /// </summary>
    public double EmissionsKgCO2 { get; set; }
}
