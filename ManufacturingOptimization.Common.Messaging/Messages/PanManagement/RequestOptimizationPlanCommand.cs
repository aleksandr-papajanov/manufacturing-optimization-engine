using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PanManagement;

public class RequestOptimizationPlanCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Motor request submitted by customer.
    /// </summary>
    public OptimizationRequest Request { get; set; } = new();
}