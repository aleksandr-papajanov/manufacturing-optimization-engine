using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;

/// <summary>
/// Published when multiple optimization strategies are ready for customer selection.
/// Contains all generated strategies with different optimization priorities.
/// </summary>
public class MultipleStrategiesReadyEvent : IMessage, IEvent
{
    /// <summary>
    /// Command that triggered this event (for correlation).
    /// </summary>
    public Guid CommandId { get; set; }
    
    /// <summary>
    /// Request ID for tracking.
    /// </summary>
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// List of optimization strategies for the customer to choose from.
    /// Typically includes: Budget, Express, Premium, and Eco strategies.
    /// </summary>
    public List<OptimizationStrategy> Strategies { get; set; } = new();
    
    /// <summary>
    /// Workflow type (Upgrade or Refurbish).
    /// </summary>
    public string WorkflowType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether strategy generation was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// Errors encountered during strategy generation (if any).
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Timestamp when strategies were generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
