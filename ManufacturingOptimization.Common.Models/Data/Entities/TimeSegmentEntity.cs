namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Database entity for a time segment within a process step execution.
/// </summary>
public class TimeSegmentEntity
{
    public Guid Id { get; set; }
    public Guid AllocatedSlotId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int SegmentOrder { get; set; }
    public string SegmentType { get; set; } = string.Empty;

    // Navigation property
    public AllocatedSlotEntity AllocatedSlot { get; set; } = null!;
}
