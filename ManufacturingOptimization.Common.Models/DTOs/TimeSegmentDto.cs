namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// DTO for a time segment within a process execution.
/// </summary>
public class TimeSegmentDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int SegmentOrder { get; set; }
    public string SegmentType { get; set; } = string.Empty;
}
