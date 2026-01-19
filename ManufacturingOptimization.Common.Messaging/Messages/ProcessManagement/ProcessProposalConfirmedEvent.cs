using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Confirmation from provider that the process has been accepted and scheduled.
/// </summary>
public class ProcessProposalConfirmedEvent : BaseEvent
{
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public Guid PlanId { get; set; }
    
    /// <summary>
    /// Scheduled start time for the process (if available).
    /// </summary>
    public DateTime? ScheduledStartTime { get; set; }
    
    public string Notes { get; set; } = string.Empty;
}
