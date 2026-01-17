using Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Engineering Design Firm provider (TP2).
/// Configure via environment variables: EngineeringDesignFirm__ProviderId, EngineeringDesignFirm__ProviderName
/// </summary>
public class EngineeringDesignFirmSettings
{
    public const string SectionName = "EngineeringDesignFirm";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    public List<ProcessCapability> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}
