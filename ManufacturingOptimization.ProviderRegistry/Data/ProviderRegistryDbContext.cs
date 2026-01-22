using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderRegistry.Data;

/// <summary>
/// Database context for provider registry.
/// </summary>
public class ProviderRegistryDbContext : DbContext
{

    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    
    public ProviderRegistryDbContext(DbContextOptions<ProviderRegistryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Provider configuration
        modelBuilder.Entity<ProviderEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Enabled).IsRequired();

            entity.HasMany(e => e.ProcessCapabilities)
                .WithOne(pc => pc.Provider)
                .HasForeignKey(pc => pc.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TechnicalCapabilities)
                .WithOne(tc => tc.Provider)
                .HasForeignKey<TechnicalCapabilitiesEntity>(tc => tc.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessCapability configuration
        modelBuilder.Entity<ProcessCapabilityEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Process).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CostPerHour).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.SpeedMultiplier).IsRequired();
            entity.Property(e => e.QualityScore).IsRequired();
            entity.Property(e => e.EnergyConsumptionKwhPerHour).IsRequired();
            entity.Property(e => e.CarbonIntensityKgCO2PerKwh).IsRequired();
            entity.Property(e => e.UsesRenewableEnergy).IsRequired();
        });

        // TechnicalCapabilities configuration
        modelBuilder.Entity<TechnicalCapabilitiesEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AxisHeight).IsRequired();
            entity.Property(e => e.Power).IsRequired();
            entity.Property(e => e.Tolerance).IsRequired();
        });
    }
}
