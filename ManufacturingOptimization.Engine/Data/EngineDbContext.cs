using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data;

/// <summary>
/// Database context for Engine service.
/// </summary>
public class EngineDbContext : DbContext
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

    public EngineDbContext(DbContextOptions<EngineDbContext> options) : base(options)
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

        // OptimizationPlan configuration
        modelBuilder.Entity<OptimizationPlanEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired();
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.SelectedStrategy)
                .WithOne(s => s.Plan)
                .HasForeignKey<OptimizationStrategyEntity>(s => s.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OptimizationStrategy configuration
        modelBuilder.Entity<OptimizationStrategyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired();
            entity.Property(e => e.StrategyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.WorkflowType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasMany(e => e.Steps)
                .WithOne(s => s.Strategy)
                .HasForeignKey(s => s.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Metrics)
                .WithOne(m => m.Strategy)
                .HasForeignKey<OptimizationMetricsEntity>(m => m.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Warranty)
                .WithOne(w => w.Strategy)
                .HasForeignKey<WarrantyTermsEntity>(w => w.StrategyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessStep configuration
        modelBuilder.Entity<ProcessStepEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StepNumber).IsRequired();
            entity.Property(e => e.Process).IsRequired().HasMaxLength(50);
            entity.Property(e => e.SelectedProviderId).IsRequired();
            entity.Property(e => e.SelectedProviderName).IsRequired().HasMaxLength(200);

            entity.HasOne(e => e.Estimate)
                .WithOne(est => est.ProcessStep)
                .HasForeignKey<ProcessEstimateEntity>(est => est.ProcessStepId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProcessEstimate configuration
        modelBuilder.Entity<ProcessEstimateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cost).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Duration).IsRequired();
            entity.Property(e => e.QualityScore).IsRequired();
            entity.Property(e => e.EmissionsKgCO2).IsRequired();
        });

        // OptimizationMetrics configuration
        modelBuilder.Entity<OptimizationMetricsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalCost).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TotalTime).IsRequired();
            entity.Property(e => e.AverageQuality).IsRequired();
            entity.Property(e => e.TotalEmissionsKgCO2).IsRequired();
            entity.Property(e => e.SolverStatus).HasMaxLength(50);
            entity.Property(e => e.ObjectiveValue).IsRequired();
        });

        // WarrantyTerms configuration
        modelBuilder.Entity<WarrantyTermsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Level).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DurationMonths).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IncludesInsurance).IsRequired();
        });
    }
}
