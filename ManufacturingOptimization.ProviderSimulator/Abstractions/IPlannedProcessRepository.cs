using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions; 

public interface IPlannedProcessRepository : IRepository<PlannedProcessEntity>
{
    /// <summary>
    /// Gets all planned processes for a provider that overlap with the given time window.
    /// </summary>
    Task<List<PlannedProcessEntity>> GetAllInTimeWindowAsync(Guid providerId, DateTime startTime, DateTime endTime);
}
