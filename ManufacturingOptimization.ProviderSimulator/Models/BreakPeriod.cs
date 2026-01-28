namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Represents a break period during the work day.
/// </summary>
public class BreakPeriod
{
    /// <summary>
    /// Start hour of the break (0-23).
    /// </summary>
    public int StartHour { get; set; }
    
    /// <summary>
    /// Start minute of the break (0-59).
    /// </summary>
    public int StartMinute { get; set; }
    
    /// <summary>
    /// Duration of the break in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }
    
    /// <summary>
    /// Name/description of the break (e.g., "Coffee break", "Team meeting").
    /// </summary>
    public string Name { get; set; } = "Break";
    
    /// <summary>
    /// Gets the start time for a specific date.
    /// </summary>
    public DateTime GetStartTime(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, StartHour, StartMinute, 0);
    }
    
    /// <summary>
    /// Gets the end time for a specific date.
    /// </summary>
    public DateTime GetEndTime(DateTime date)
    {
        return GetStartTime(date).AddMinutes(DurationMinutes);
    }
    
    /// <summary>
    /// Checks if a given time falls within this break period.
    /// </summary>
    public bool Contains(DateTime time)
    {
        var start = GetStartTime(time.Date);
        var end = GetEndTime(time.Date);
        return time >= start && time < end;
    }
}
