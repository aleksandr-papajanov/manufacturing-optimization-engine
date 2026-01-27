using System;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ProcessEstimateEntity
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public decimal Cost { get; set; }
    public long DurationTicks { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }

    // Navigation property
    public ProposalEntity Proposal { get; set; } = null!;
}
