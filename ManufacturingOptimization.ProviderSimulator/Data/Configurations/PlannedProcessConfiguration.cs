using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class PlannedProcessConfiguration : IEntityTypeConfiguration<PlannedProcessEntity>
{
    public void Configure(EntityTypeBuilder<PlannedProcessEntity> entity)
    {
        entity.ToTable("PlannedProcesses");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProposalId)
            .IsRequired();
    }
}