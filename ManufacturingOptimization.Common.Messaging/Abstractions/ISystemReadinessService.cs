namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Service that coordinates system startup and blocks operations until all services are ready.
/// </summary>
public interface ISystemReadinessService
{
    /// <summary>
    /// Wait until the system is fully ready (all required services reported ready).
    /// </summary>
    Task WaitForSystemReadyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks the system as ready. Called when SystemReadyEvent is received.
    /// </summary>
    void MarkSystemReady();
    
    /// <summary>
    /// Checks if the system is ready without blocking.
    /// </summary>
    bool IsSystemReady { get; }
}
