namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Engineering Design Firm provider (TP2).
/// Configure via environment variables: EngineeringDesignFirm__ProviderId, EngineeringDesignFirm__ProviderName
/// Capabilities: Redesign
/// </summary>
public class EngineeringDesignFirmSettings
{
    public const string SectionName = "EngineeringDesignFirm";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    public List<string> Capabilities { get; set; } = new();
    public double AxisHeight { get; set; } = 0.0;
    public double Power { get; set; } = 0.0;
    public double Tolerance { get; set; } = 0.0;
}
