using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Request sent to a provider to get cost, time, and quality estimates for a specific process.
/// </summary>
public class RequestProcessEstimateCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public string Activity { get; set; } = string.Empty;
    public MotorSpecifications MotorSpecs { get; set; } = new();
    
    /// <summary>
    /// Queue name where the response should be sent.
    /// </summary>
    public string ReplyTo { get; set; } = string.Empty;
}
