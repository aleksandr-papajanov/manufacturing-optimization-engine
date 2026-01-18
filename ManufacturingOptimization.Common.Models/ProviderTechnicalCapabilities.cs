namespace Common.Models;

/// <summary>
/// Technical capabilities/requirements of a provider.
/// </summary>
public class ProviderTechnicalCapabilities
{
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
