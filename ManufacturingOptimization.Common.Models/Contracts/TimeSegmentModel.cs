using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Contracts;

/// <summary>
/// Represents a single time segment within a process execution.
/// A process may consist of multiple segments due to breaks, lunch, etc.
/// </summary>
public class TimeSegmentModel
{
    /// <summary>
    /// Start time of this segment (inclusive).
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// End time of this segment (exclusive).
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Order of this segment within the process (0-based).
    /// </summary>
    public int SegmentOrder { get; set; }
    
    /// <summary>
    /// Type of segment (working time or break).
    /// </summary>
    public SegmentType SegmentType { get; set; }
    
    /// <summary>
    /// Duration of this segment.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;
}
