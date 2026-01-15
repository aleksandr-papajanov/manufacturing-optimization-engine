namespace ManufacturingOptimization.Engine.Settings;

public class ProviderValidationSettings
{
    public const string SectionName = "ProviderValidation";

    public TechnicalLimits TechnicalLimits { get; set; } = new();
    public RequiredCapabilities RequiredCapabilities { get; set; } = new();
    public List<string> RequiredProcesses { get; set; } = new();
}

public class TechnicalLimits
{
    public double MinAxisHeight { get; set; }
    public double MaxAxisHeight { get; set; }
    public double MinPower { get; set; }
    public double MaxPower { get; set; }
    public double MinTolerance { get; set; }
    public double MaxTolerance { get; set; }
}

public class RequiredCapabilities
{
    public List<string> MainRemanufacturingCenter { get; set; } = new();
    public List<string> EngineeringDesignFirm { get; set; } = new();
    public List<string> PrecisionMachineShop { get; set; } = new();
}
