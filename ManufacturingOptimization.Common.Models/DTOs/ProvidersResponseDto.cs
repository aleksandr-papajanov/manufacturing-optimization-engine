using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Response wrapper for provider list.
/// </summary>
public class ProvidersResponseDto
{
    /// <summary>
    /// Total number of registered providers.
    /// </summary>
    public int TotalProviders { get; set; }
    
    /// <summary>
    /// List of provider details.
    /// </summary>
    public List<ProviderModel> Providers { get; set; } = new();
}
