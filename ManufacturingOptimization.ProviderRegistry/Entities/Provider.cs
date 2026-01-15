namespace ManufacturingOptimization.ProviderRegistry.Entities;

public class Provider
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public List<string> Capabilities { get; set; } = new();
    public TechnicalRequirements TechnicalRequirements { get; set; } = new();
}

public class TechnicalRequirements
{
    public double AxisHeight { get; set; }
    public double Power { get; set; }
    public double Tolerance { get; set; }
}
