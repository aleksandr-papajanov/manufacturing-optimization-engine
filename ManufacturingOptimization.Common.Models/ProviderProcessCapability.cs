using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Defines a process that a provider can perform with specific characteristics.
/// Each provider has different capabilities, costs, and environmental impact for each process.
/// </summary>
public class ProviderProcessCapability
{
    /// <summary>
    /// Process name (e.g., "Turning", "Grinding", "Cleaning").
    /// Maps from JSON "processName" field to ProcessType enum.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// Base cost per hour for this process (USD/hour).
    /// </summary>
    public decimal CostPerHour { get; set; }
    
    /// <summary>
    /// Typical duration multiplier for this provider (1.0 = standard, 0.8 = faster, 1.2 = slower).
    /// </summary>
    public double SpeedMultiplier { get; set; } = 1.0;
    
    /// <summary>
    /// Quality score for this process (0.0 - 1.0, higher is better).
    /// Based on provider's experience, equipment quality, and track record.
    /// </summary>
    public double QualityScore { get; set; } = 0.8;
    
    /// <summary>
    /// Energy consumption for this process (kWh per hour).
    /// </summary>
    public double EnergyConsumptionKwhPerHour { get; set; }
    
    /// <summary>
    /// Carbon intensity of provider's energy source (kg CO2 per kWh).
    /// Typical values: 0.2-0.3 (renewable), 0.4-0.5 (mixed), 0.6-0.8 (coal/gas).
    /// </summary>
    public double CarbonIntensityKgCO2PerKwh { get; set; } = 0.5;
    
    /// <summary>
    /// Whether this provider uses renewable energy for this process.
    /// </summary>
    public bool UsesRenewableEnergy { get; set; }
}
