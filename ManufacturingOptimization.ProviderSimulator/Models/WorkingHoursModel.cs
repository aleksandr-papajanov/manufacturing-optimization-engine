namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Defines working hours and breaks for a provider.
/// </summary>
public class WorkingHoursModel
{
    /// <summary>
    /// Start of working day (hour, 0-23). Default: 8 (8:00 AM)
    /// </summary>
    public int WorkDayStartHour { get; set; } = 8;
    
    /// <summary>
    /// End of working day (hour, 0-23). Default: 17 (5:00 PM)
    /// </summary>
    public int WorkDayEndHour { get; set; } = 17;
    
    /// <summary>
    /// Start of lunch break (hour, 0-23). Default: 12 (12:00 PM)
    /// </summary>
    public int LunchBreakStartHour { get; set; } = 12;
    
    /// <summary>
    /// Duration of lunch break in minutes. Default: 60 minutes
    /// </summary>
    public int LunchBreakDurationMinutes { get; set; } = 60;
    
    /// <summary>
    /// Working days (0 = Sunday, 6 = Saturday). Default: Monday-Friday
    /// </summary>
    public HashSet<DayOfWeek> WorkingDays { get; set; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };
    
    /// <summary>
    /// Whether this provider operates 24/7 (ignores working hours).
    /// </summary>
    public bool Is24x7 { get; set; } = false;
    
    /// <summary>
    /// Additional break periods during the day (coffee breaks, meetings, etc.).
    /// </summary>
    public List<BreakPeriod> AdditionalBreaks { get; set; } = new();
    
    /// <summary>
    /// Generates random breaks for testing purposes.
    /// </summary>
    public void GenerateRandomBreaks(int count, Random? random = null)
    {
        random ??= new Random();
        AdditionalBreaks.Clear();
        
        var breakNames = new[] { "Coffee break", "Team meeting", "Equipment maintenance", "Safety briefing", "Shift change" };
        
        for (int i = 0; i < count; i++)
        {
            var startHour = random.Next(WorkDayStartHour + 1, WorkDayEndHour - 1);
            var startMinute = random.Next(0, 4) * 15; // 0, 15, 30, 45
            var duration = random.Next(2, 6) * 5; // 10-25 minutes
            
            AdditionalBreaks.Add(new BreakPeriod
            {
                StartHour = startHour,
                StartMinute = startMinute,
                DurationMinutes = duration,
                Name = breakNames[random.Next(breakNames.Length)]
            });
        }
        
        // Sort breaks by start time
        AdditionalBreaks = AdditionalBreaks
            .OrderBy(b => b.StartHour)
            .ThenBy(b => b.StartMinute)
            .ToList();
    }
    
    /// <summary>
    /// Checks if a given time is within working hours.
    /// </summary>
    public bool IsWorkingTime(DateTime time)
    {
        if (Is24x7) return true;
        
        // Check if day is a working day
        if (!WorkingDays.Contains(time.DayOfWeek)) return false;
        
        var hour = time.Hour;
        var minute = time.Minute;
        
        // Check if before work start or after work end
        if (hour < WorkDayStartHour || hour >= WorkDayEndHour) return false;
        
        // Check if during lunch break
        var lunchStart = LunchBreakStartHour;
        var lunchEnd = lunchStart + (LunchBreakDurationMinutes / 60.0);
        var currentHourDecimal = hour + (minute / 60.0);
        
        if (currentHourDecimal >= lunchStart && currentHourDecimal < lunchEnd) return false;
        
        // Check if during any additional break
        foreach (var breakPeriod in AdditionalBreaks)
        {
            if (breakPeriod.Contains(time)) return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets the next available working time from a given time.
    /// </summary>
    public DateTime GetNextWorkingTime(DateTime time)
    {
        if (Is24x7) return time;
        
        var current = time;
        
        // Find next working day
        while (!WorkingDays.Contains(current.DayOfWeek))
        {
            current = current.Date.AddDays(1).AddHours(WorkDayStartHour);
        }
        
        // If before work start, move to work start
        if (current.Hour < WorkDayStartHour)
        {
            current = new DateTime(current.Year, current.Month, current.Day, WorkDayStartHour, 0, 0);
        }
        
        // If during lunch, move to after lunch
        var lunchStart = new DateTime(current.Year, current.Month, current.Day, LunchBreakStartHour, 0, 0);
        var lunchEnd = lunchStart.AddMinutes(LunchBreakDurationMinutes);
        
        if (current >= lunchStart && current < lunchEnd)
        {
            current = lunchEnd;
        }
        
        // If after work end, move to next work day start
        if (current.Hour >= WorkDayEndHour)
        {
            current = current.Date.AddDays(1).AddHours(WorkDayStartHour);
            return GetNextWorkingTime(current); // Recursive call to handle weekend
        }
        
        return current;
    }
    
    /// <summary>
    /// Gets the end of working hours for a given date.
    /// </summary>
    public DateTime GetWorkDayEnd(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, WorkDayEndHour, 0, 0);
    }
    
    /// <summary>
    /// Calculates actual working hours between two times, excluding breaks and non-working hours.
    /// </summary>
    public TimeSpan GetWorkingDuration(DateTime start, DateTime end)
    {
        if (Is24x7) return end - start;
        
        var totalMinutes = 0.0;
        var current = GetNextWorkingTime(start);
        
        while (current < end)
        {
            var dayEnd = GetWorkDayEnd(current);
            var segmentEnd = end < dayEnd ? end : dayEnd;
            
            // Calculate minutes in this segment, excluding lunch
            var lunchStart = new DateTime(current.Year, current.Month, current.Day, LunchBreakStartHour, 0, 0);
            var lunchEnd = lunchStart.AddMinutes(LunchBreakDurationMinutes);
            
            if (current < lunchStart && segmentEnd > lunchStart)
            {
                // Segment spans lunch break
                totalMinutes += (lunchStart - current).TotalMinutes;
                
                if (segmentEnd > lunchEnd)
                {
                    totalMinutes += (segmentEnd - lunchEnd).TotalMinutes;
                }
            }
            else if (current >= lunchEnd || segmentEnd <= lunchStart)
            {
                // Segment doesn't overlap with lunch
                totalMinutes += (segmentEnd - current).TotalMinutes;
            }
            
            // Move to next work day
            current = GetNextWorkingTime(dayEnd.AddMinutes(1));
        }
        
        return TimeSpan.FromMinutes(totalMinutes);
    }
}
