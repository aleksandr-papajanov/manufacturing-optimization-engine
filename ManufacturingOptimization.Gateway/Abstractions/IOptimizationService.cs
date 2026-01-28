using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IOptimizationService
    {
        Task<Guid> RequestOptimizationPlanAsync(OptimizationRequestDto request);
        Task SelectStrategyAsync(Guid requestId, Guid strategyId);
        Task<List<OptimizationStrategyDto>> GetStrategiesAsync(Guid requestId);
        Task<OptimizationPlanDto> GetPlanAsync(Guid requestId);
    }
}
