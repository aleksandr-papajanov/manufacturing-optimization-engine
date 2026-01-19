using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Propose a process to a provider for preliminary acceptance/rejection.
/// Provider can accept with estimates or decline.
/// </summary>
public class ProposeProcessToProviderCommand : BaseRequestReplyCommand
{
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public MotorSpecifications MotorSpecs { get; set; } = new();
}
