namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Response wrapper for provider list.
/// </summary>
public class ProvidersResponse
{
    /// <summary>
    /// Total number of registered providers.
    /// </summary>
    public int TotalProviders { get; set; }
    
    /// <summary>
    /// List of provider details.
    /// </summary>
    public List<Provider> Providers { get; set; } = new();
}
