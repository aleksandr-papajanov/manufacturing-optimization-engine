namespace ManufacturingOptimization.Engine.Abstractions;

public class RegisteredProvider
{
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
}

public interface IProviderRepository
{
    int Count { get; }
    void Create(Guid providerId, string providerType, string providerName);
    List<RegisteredProvider> GetAll();
}
