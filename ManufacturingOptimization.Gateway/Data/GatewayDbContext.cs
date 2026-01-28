using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Configurations;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data;

/// <summary>
/// Database context for Gateway service.
/// Stores optimization plans and strategies.
/// </summary>
public class GatewayDbContext : DbContext, IOptimizationDbContext, IProviderDbContext
{
    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    public DbSet<OptimizationPlanEntity> OptimizationPlans => Set<OptimizationPlanEntity>();
    public DbSet<OptimizationStrategyEntity> OptimizationStrategies => Set<OptimizationStrategyEntity>();
    public DbSet<ProcessStepEntity> ProcessSteps => Set<ProcessStepEntity>();
    public DbSet<ProcessEstimateEntity> ProcessEstimates => Set<ProcessEstimateEntity>();
    public DbSet<OptimizationMetricsEntity> OptimizationMetrics => Set<OptimizationMetricsEntity>();
    public DbSet<WarrantyTermsEntity> WarrantyTerms => Set<WarrantyTermsEntity>();
    public DbSet<AllocatedSlotEntity> AllocatedSlots => Set<AllocatedSlotEntity>();
    public DbSet<TimeSegmentEntity> TimeSegments => Set<TimeSegmentEntity>();

    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessCapabilityConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalCapabilitiesConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationPlanConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessStepConfiguration());
        modelBuilder.ApplyConfiguration(new AllocatedSlotConfiguration());
        modelBuilder.ApplyConfiguration(new TimeSegmentConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessEstimateConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationMetricsConfiguration());
        modelBuilder.ApplyConfiguration(new WarrantyTermsConfiguration());
    }
}
