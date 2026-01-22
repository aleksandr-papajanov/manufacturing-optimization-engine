using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;
public class OptimizationStrategyConfiguration : IEntityTypeConfiguration<OptimizationStrategyEntity>
{
    public void Configure(EntityTypeBuilder<OptimizationStrategyEntity> entity)
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
    }
}