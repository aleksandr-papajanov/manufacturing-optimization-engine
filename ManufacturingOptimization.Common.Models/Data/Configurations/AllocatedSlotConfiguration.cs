using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;

public class AllocatedSlotConfiguration : IEntityTypeConfiguration<AllocatedSlotEntity>
{
    public void Configure(EntityTypeBuilder<AllocatedSlotEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.StartTime).IsRequired();
        entity.Property(e => e.EndTime).IsRequired();

        entity.HasMany(e => e.Segments)
            .WithOne(s => s.AllocatedSlot)
            .HasForeignKey(s => s.AllocatedSlotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
