using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProvidersResponseDto
{
    public int TotalProviders { get; set; }
    public List<ProviderModel> Providers { get; set; } = new();
}
