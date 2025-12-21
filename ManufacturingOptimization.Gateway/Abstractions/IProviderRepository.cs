namespace ManufacturingOptimization.Gateway.Abstractions;

public class RegisteredProvider
{
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}

public interface IProviderRepository
{
    void Create(Guid providerId, string providerType, string providerName);
    List<RegisteredProvider> GetAll();
    RegisteredProvider? GetById(Guid providerId);
}
