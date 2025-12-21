namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Precision Machine Shop provider.
/// Configure via environment variables: PrecisionMachineShop__ProviderId, PrecisionMachineShop__ProviderName
/// </summary>
public class PrecisionMachineShopSettings
{
    public const string SectionName = "PrecisionMachineShop";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
}
