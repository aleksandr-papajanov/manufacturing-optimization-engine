namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Process estimate entity for database storage.
/// </summary>
public class ProcessEstimateEntity
{
    public Guid Id { get; set; }
    public Guid ProcessStepId { get; set; }
    public decimal Cost { get; set; }
    public long Duration { get; set; } // Stored as Ticks
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public string? AvailableTimeSlotsJson { get; set; }

    // Navigation property
    public ProcessStepEntity ProcessStep { get; set; } = null!;
}
