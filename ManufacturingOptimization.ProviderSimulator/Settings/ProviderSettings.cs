using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Main Remanufacturing Center provider (TP1).
/// Configure via environment variables: Provider__ProviderId, Provider__ProviderName
/// </summary>
public class ProviderSettings
{
    public const string SectionName = "Provider";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;

    public List<ProviderProcessCapability> ProcessCapabilities { get; set; } = new();
    public ProviderTechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}
