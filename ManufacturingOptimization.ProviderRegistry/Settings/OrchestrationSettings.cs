namespace ManufacturingOptimization.ProviderRegistry;

/// <summary>
/// Settings for provider orchestration mode.
/// Value should be provided via environment variable (Orchestration__Mode).
/// </summary>
public class OrchestrationSettings
{
    public const string SectionName = "Orchestration";
    
    public string Mode { get; set; } = "Production";
    public bool IsOrchestrationEnabled => Mode.Equals("Production", StringComparison.OrdinalIgnoreCase);
}
