using ManufacturingOptimization.Common.Models.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IProviderService
    {
        Task<List<ProviderDto>> GetProvidersAsync();
    }
}
