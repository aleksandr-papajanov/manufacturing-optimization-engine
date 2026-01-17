namespace ManufacturingOptimization.Engine.Abstractions;

public class RegisteredProvider
{
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    
    public double AxisHeight { get; set; }
    public double Power { get; set; }
    public double Tolerance { get; set; }
}

public interface IProviderRepository
{
    int Count { get; }
    void Create(Guid providerId, string providerType, string providerName, List<string> capabilities, 
        double axisHeight = 0, double power = 0, double tolerance = 0);
    List<RegisteredProvider> GetAll();
    
    /// <summary>
    /// Find all providers that have the specified capability.
    /// </summary>
    List<RegisteredProvider> FindByCapability(string capability);
}
