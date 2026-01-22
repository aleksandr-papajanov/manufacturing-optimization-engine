using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderDbContext : IDbContext
{
    public DbSet<ProviderEntity> Providers { get; }
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities { get; }
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities { get; }
}