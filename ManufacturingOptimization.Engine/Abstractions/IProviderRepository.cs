using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
    /// <summary>
    /// Find all providers that can perform the specified process.
    /// Returns providers with their ProcessCapability for that process.
    /// </summary>
    Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process);
}
