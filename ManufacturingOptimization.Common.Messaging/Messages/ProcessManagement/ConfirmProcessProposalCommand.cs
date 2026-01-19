using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Final confirmation of accepted process proposal to provider after strategy selection.
/// </summary>
public class ConfirmProcessProposalCommand : BaseRequestReplyCommand
{
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// The optimization plan ID this process is part of.
    /// </summary>
    public Guid PlanId { get; set; }
}
