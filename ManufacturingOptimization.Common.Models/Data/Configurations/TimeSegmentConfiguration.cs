using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;

public class TimeSegmentConfiguration : IEntityTypeConfiguration<TimeSegmentEntity>
{
    public void Configure(EntityTypeBuilder<TimeSegmentEntity> builder)
    {
        builder.HasKey(e => e.Id); 
        builder.Property(e => e.StartTime).IsRequired();  
        builder.Property(e => e.EndTime).IsRequired(); 
        builder.Property(e => e.SegmentOrder).IsRequired();
        builder.Property(e => e.SegmentType).IsRequired().HasMaxLength(20);

        builder.HasOne(e => e.AllocatedSlot)
            .WithMany(p => p.Segments)
            .HasForeignKey(e => e.AllocatedSlotId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(e => new { e.AllocatedSlotId, e.SegmentOrder })
            .IsUnique();
    }
}
