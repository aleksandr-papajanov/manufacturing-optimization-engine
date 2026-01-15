namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Precision Machine Shop provider (TP3).
/// Configure via environment variables: PrecisionMachineShop__ProviderId, PrecisionMachineShop__ProviderName
/// Capabilities: Turning, Grinding
/// </summary>
public class PrecisionMachineShopSettings
{
    public const string SectionName = "PrecisionMachineShop";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    public List<string> Capabilities { get; set; } = new();
    public double AxisHeight { get; set; } = 0.0;
    public double Power { get; set; } = 0.0;
    public double Tolerance { get; set; } = 0.0;
}
