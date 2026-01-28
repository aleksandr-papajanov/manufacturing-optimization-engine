using ManufacturingOptimization.Common.Messaging.Abstractions;
using System;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Command sent by the Engine to a Provider to begin physical execution of a process step.
/// Represents the transition from Planning -> Doing.
/// </summary>
public class ExecuteProcessCommand : BaseCommand
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public int StepIndex { get; set; }
    
    /// <summary>
    /// The specific process to perform (e.g. "Rewind", "Cleaning").
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;
    
    /// <summary>
    /// The Provider assigned to perform this step.
    /// </summary>
    public Guid TargetProviderId { get; set; }
    
    /// <summary>
    /// Expected duration (used for simulation delays).
    /// </summary>
    public double DurationHours { get; set; }
}