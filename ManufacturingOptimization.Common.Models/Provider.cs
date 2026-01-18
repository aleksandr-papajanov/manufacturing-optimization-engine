namespace Common.Models;

/// <summary>
/// Unified provider model used across all services.
/// </summary>
public class Provider
{
    /// <summary>
    /// Unique provider identifier.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Provider type (e.g., "MainRemanufacturingCenter", "PrecisionMachineShop").
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether provider is enabled for processing.
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// List of processes this provider can perform, with specific characteristics for each.
    /// Each capability includes cost, quality, energy consumption, and emissions data.
    /// </summary>
    public List<ProcessCapability> ProcessCapabilities { get; set; } = new();
    
    /// <summary>
    /// Technical specifications (axis height, power, tolerance).
    /// </summary>
    public ProviderTechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}
