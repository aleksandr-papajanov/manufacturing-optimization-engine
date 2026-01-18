using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Main Remanufacturing Center provider (TP1).
/// Configure via environment variables: MainRemanufacturingCenter__ProviderId, MainRemanufacturingCenter__ProviderName
/// </summary>
public class RemanufacturingCenterSettings
{
    public const string SectionName = "MainRemanufacturingCenter";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;

    public List<ProviderProcessCapability> ProcessCapabilities { get; set; } = new();
    public ProviderTechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}
