using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;

/// <summary>
/// Response from provider with estimates for the requested process.
/// </summary>
public class ProcessEstimateResponse : IMessage, IEvent
{
    public Guid ResponseId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public string Activity { get; set; } = string.Empty;
    
    /// <summary>
    /// Estimated cost in currency.
    /// </summary>
    public decimal CostEstimate { get; set; }
    
    /// <summary>
    /// Estimated time to complete.
    /// </summary>
    public TimeSpan TimeEstimate { get; set; }
    
    /// <summary>
    /// Quality score (0.0 - 1.0).
    /// </summary>
    public double QualityScore { get; set; }
    
    /// <summary>
    /// Estimated carbon emissions in kg CO2.
    /// Calculated as: EnergyConsumption (kWh) × CarbonIntensity (kg CO2/kWh).
    /// </summary>
    public double EmissionsKgCO2 { get; set; }
    
    /// <summary>
    /// ID of the command that triggered this response (for correlation).
    /// </summary>
    public Guid CommandId { get; set; }
    
    /// <summary>
    /// Optional additional information.
    /// </summary>
    public string? Notes { get; set; }
}
