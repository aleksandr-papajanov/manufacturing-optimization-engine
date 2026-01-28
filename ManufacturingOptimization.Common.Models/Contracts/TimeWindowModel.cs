namespace ManufacturingOptimization.Common.Models.Contracts;

/// <summary>
/// Represents a time window for scheduling optimization and production processes.
/// Defines the period during which a process should start or execute.
/// </summary>
public class TimeWindowModel
{
    /// <summary>
    /// The start of the time window.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// The end of the time window.
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Gets the duration of the time window.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Time segments within this window (working time and breaks).
    /// Populated by providers when calculating available slots.
    /// </summary>
    public List<TimeSegmentModel> Segments { get; set; } = new();
}
