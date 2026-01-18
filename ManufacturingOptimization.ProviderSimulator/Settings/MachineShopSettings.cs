using Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Precision Machine Shop provider (TP3).
/// Configure via environment variables: PrecisionMachineShop__ProviderId, PrecisionMachineShop__ProviderName
/// </summary>
public class MachineShopSettings
{
    public const string SectionName = "PrecisionMachineShop";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    public List<ProcessCapability> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}
