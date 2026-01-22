using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
    /// <summary>
    /// Find all providers that can perform the specified process.
    /// Returns providers with their ProcessCapability for that process.
    /// </summary>
    Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process);
}
