namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Main Remanufacturing Center provider (TP1).
/// Configure via environment variables: MainRemanufacturingCenter__ProviderId, MainRemanufacturingCenter__ProviderName
/// Capabilities: Cleaning, Disassembly, PartSub, Reassembly, Certification
/// </summary>
public class MainRemanufacturingCenterSettings
{
    public const string SectionName = "MainRemanufacturingCenter";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    public List<string> Capabilities { get; set; } = new();
    public double AxisHeight { get; set; } = 0.0;
    public double Power { get; set; } = 0.0;
    public double Tolerance { get; set; } = 0.0;
}
