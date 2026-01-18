namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Status of the manufacturing plan execution.
/// </summary>
public enum OptimizationPlanStatus
{
    /// <summary>
    /// Plan is being created, strategies are being generated.
    /// </summary>
    Draft,
    
    /// <summary>
    /// Strategies ready, waiting for customer selection.
    /// </summary>
    AwaitingSelection,
    
    /// <summary>
    /// Customer has selected a strategy, awaiting confirmation.
    /// </summary>
    Selected,
    
    /// <summary>
    /// Plan confirmed, sent to providers for execution.
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Manufacturing is in progress.
    /// </summary>
    InProgress,
    
    /// <summary>
    /// All steps completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Plan was cancelled.
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Plan failed during execution.
    /// </summary>
    Failed
}

