using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderSimulatorDbContext : IDbContext
{
    DbSet<PlannedProcessEntity> PlannedProcesses { get; }
    DbSet<ProposalEntity> Proposals { get; }
}