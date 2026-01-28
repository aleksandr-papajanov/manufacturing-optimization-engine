namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Database entity for an allocated time slot with detailed segment breakdown.
/// </summary>
public class AllocatedSlotEntity
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Navigation properties
    public List<TimeSegmentEntity> Segments { get; set; } = new();
}
