namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Engineering Design Firm provider.
/// Configure via environment variables: EngineeringDesignFirm__ProviderId, EngineeringDesignFirm__ProviderName
/// </summary>
public class EngineeringDesignFirmSettings
{
    public const string SectionName = "EngineeringDesignFirm";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
}
