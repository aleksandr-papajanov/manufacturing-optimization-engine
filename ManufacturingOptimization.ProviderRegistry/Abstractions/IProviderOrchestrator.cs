using Common.Models;

namespace ManufacturingOptimization.ProviderRegistry.Abstractions;

/// <summary>
/// Provider orchestration interface.
/// Production mode: creates containers via Docker API.
/// Development mode: no-op, providers managed by docker-compose.
/// </summary>
public interface IProviderOrchestrator
{
    Task StartAllAsync(CancellationToken cancellationToken = default);
    Task StartAsync(Provider provider, CancellationToken cancellationToken = default);
    Task StopAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task StopAllAsync(CancellationToken cancellationToken = default);
    Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default);
}