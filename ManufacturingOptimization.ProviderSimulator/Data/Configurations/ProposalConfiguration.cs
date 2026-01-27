using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<ProposalEntity>
{
    public void Configure(EntityTypeBuilder<ProposalEntity> builder)
    {
        builder.ToTable("Proposals");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.RequestId).IsRequired();
        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.Process).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ArrivedAt).IsRequired();
        
        // Owned type для MotorSpecifications (встроенные данные)
        builder.OwnsOne(x => x.MotorSpecs, ms =>
        {
            ms.Property(m => m.PowerKW).IsRequired();
            ms.Property(m => m.AxisHeightMM).IsRequired();
            ms.Property(m => m.CurrentEfficiency).IsRequired().HasMaxLength(50);
            ms.Property(m => m.TargetEfficiency).IsRequired().HasMaxLength(50);
            ms.Property(m => m.MalfunctionDescription).HasMaxLength(500);
        });
        
        // Связь 1:1 с ProcessEstimateEntity
        builder.HasOne(x => x.Estimate)
            .WithOne(x => x.Proposal)
            .HasForeignKey<ProcessEstimateEntity>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Связь 1:1 с PlannedProcessEntity
        builder.HasOne(x => x.PlannedProcess)
            .WithOne(x => x.Proposal)
            .HasForeignKey<PlannedProcessEntity>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
