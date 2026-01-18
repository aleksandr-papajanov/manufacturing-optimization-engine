namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Standard values for manufacturing processes.
/// </summary>
public class ProcessStandardsSettings
{
    public const string SectionName = "ProcessStandards";
    
    /// <summary>
    /// Standard duration for each process type in hours.
    /// </summary>
    public Dictionary<string, double> StandardDurationHours { get; set; } = new();
}
