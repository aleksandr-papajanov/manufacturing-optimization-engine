namespace Common.Models;

/// <summary>
/// Standard process types in motor remanufacturing.
/// </summary>
public static class ProcessType
{
    public const string Cleaning = "Cleaning";
    public const string Disassembly = "Disassembly";
    public const string Redesign = "Redesign";
    public const string Turning = "Turning";
    public const string Grinding = "Grinding";
    public const string PartSubstitution = "PartSubstitution";
    public const string Reassembly = "Reassembly";
    public const string Certification = "Certification";
    
    /// <summary>
    /// Standard duration for each process type (hours).
    /// These are baseline values that providers can modify with their SpeedMultiplier.
    /// </summary>
    public static readonly Dictionary<string, double> StandardDurationHours = new()
    {
        { Cleaning, 4.0 },
        { Disassembly, 6.0 },
        { Redesign, 16.0 },
        { Turning, 8.0 },
        { Grinding, 6.0 },
        { PartSubstitution, 3.0 },
        { Reassembly, 8.0 },
        { Certification, 2.0 }
    };
    
    /// <summary>
    /// Standard energy consumption for each process type (kWh).
    /// Based on typical industrial equipment power ratings and usage.
    /// </summary>
    public static readonly Dictionary<string, double> StandardEnergyConsumptionKwh = new()
    {
        { Cleaning, 2.0 },    // Pressure washers, degreasing
        { Disassembly, 1.5 }, // Hand tools, light machinery
        { Redesign, 0.5 },    // Engineering workstations
        { Turning, 5.0 },     // CNC lathes, high power
        { Grinding, 4.0 },    // Grinding machines
        { PartSubstitution, 1.0 }, // Assembly tools
        { Reassembly, 2.0 },  // Assembly equipment
        { Certification, 1.0 } // Testing equipment
    };
}
