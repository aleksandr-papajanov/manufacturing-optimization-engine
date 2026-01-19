using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;

/// <summary>
/// Published when multiple optimization strategies are ready for customer selection.
/// Contains all generated strategies with different optimization priorities.
/// </summary>
public class MultipleStrategiesReadyEvent : BaseEvent
{
    /// <summary>
    /// Request ID for tracking.
    /// </summary>
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// List of optimization strategies for the customer to choose from.
    /// Typically includes: Budget, Express, Premium, and Eco strategies.
    /// </summary>
    public List<OptimizationStrategy> Strategies { get; set; } = [];
    
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
    public List<string> Errors { get; set; } = [];
    
    /// <summary>
    /// Timestamp when strategies were generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
