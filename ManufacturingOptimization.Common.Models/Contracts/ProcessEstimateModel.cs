namespace ManufacturingOptimization.Common.Models.Contracts;

/// <summary>
/// Represents a provider's estimate for executing a specific process.
/// Contains cost, time, quality, and environmental impact metrics.
/// </summary>
public class ProcessEstimateModel
{
    /// <summary>
    /// Unique identifier for this estimate.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the unique identifier for the proposal.
    /// </summary>
    public Guid ProposalId { get; set; }

    /// <summary>
    /// Estimated cost in currency.
    /// </summary>
    public decimal Cost { get; set; }
    
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
    
    /// <summary>
    /// Available time slots when the provider can execute the process.
    /// Empty list means no available slots within the requested time window.
    /// </summary>
    public List<TimeWindowModel> AvailableTimeSlots { get; set; } = new();
}
