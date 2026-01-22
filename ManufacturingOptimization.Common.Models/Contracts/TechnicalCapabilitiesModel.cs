namespace ManufacturingOptimization.Common.Models.Contracts;

/// <summary>
/// Technical capabilities/requirements of a provider.
/// </summary>
public class TechnicalCapabilitiesModel
{
    /// <summary>
    /// Unique identifier for these capabilities.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Maximum axis height the provider can handle (mm).
    /// </summary>
    public double AxisHeight { get; set; }
    
    /// <summary>
    /// Maximum power the provider can handle (kW).
    /// </summary>
    public double Power { get; set; }
    
    /// <summary>
    /// Tolerance/precision level (mm).
    /// </summary>
    public double Tolerance { get; set; }
}
