using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;
public class OptimizationPlanConfiguration : IEntityTypeConfiguration<OptimizationPlanEntity>
{
    public void Configure(EntityTypeBuilder<OptimizationPlanEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.RequestId).IsRequired();
        entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        entity.Property(e => e.CreatedAt).IsRequired();

        entity.HasOne(e => e.SelectedStrategy)
            .WithOne(s => s.Plan)
            .HasForeignKey<OptimizationStrategyEntity>(s => s.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}