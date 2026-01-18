using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

public interface IProviderRepository
{
    int Count { get; }
    
    /// <summary>
    /// Create a new provider in the repository.
    /// </summary>
    void Create(Provider provider);
    
    /// <summary>
    /// Get all registered providers.
    /// </summary>
    List<Provider> GetAll();
    
    /// <summary>
    /// Find all providers that can perform the specified process.
    /// Returns providers with their ProcessCapability for that process.
    /// </summary>
    List<(Provider Provider, ProviderProcessCapability Capability)> FindByProcess(string processName);
}
