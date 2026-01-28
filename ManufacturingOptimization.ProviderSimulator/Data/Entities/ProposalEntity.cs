using System;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ProposalEntity
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public Guid? EstimateId { get; set; }
    public Guid? PlannedProcessId { get; set; }
    public ProcessType Process { get; set; }
    public ProposalStatus Status { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public MotorSpecificationsEntity MotorSpecs { get; set; } = new();

    // Navigation properties
    public ProcessEstimateEntity? Estimate { get; set; }
    public PlannedProcessEntity? PlannedProcess { get; set; }
}