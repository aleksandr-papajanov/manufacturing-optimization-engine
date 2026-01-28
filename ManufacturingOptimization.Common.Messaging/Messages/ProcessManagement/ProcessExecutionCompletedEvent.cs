using ManufacturingOptimization.Common.Messaging.Abstractions;
using System;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Event published by a Provider when a process step is finished.
/// Signals the Engine to update the context and move to the next step.
/// </summary>
public class ProcessExecutionCompletedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public Guid ProviderId { get; set; }
    
    public bool Success { get; set; }
    public string OutputResult { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
}