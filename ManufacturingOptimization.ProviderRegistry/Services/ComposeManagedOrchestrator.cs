using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Entities;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Development mode orchestrator - providers managed by docker-compose.
/// All operations are no-op except cleanup of production containers.
/// </summary>
public class ComposeManagedOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    public ComposeManagedOrchestrator(ILogger<ComposeManagedOrchestrator> logger)
        : base(logger)
    {
    }

    public Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: providers managed by docker-compose");
        return Task.CompletedTask;
    }

    public Task StartAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: provider {Type} managed by docker-compose", provider.Type);
        return Task.CompletedTask;
    }

    public Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: providers managed by docker-compose");
        return Task.CompletedTask;
    }

    public Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: providers managed by docker-compose");
        return Task.CompletedTask;
    }
}