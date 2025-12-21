using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.ProviderRegistry.Abstractions;

namespace ManufacturingOptimization.ProviderRegistry;

public class ProviderRegistryWorker : BackgroundService
{
    private readonly ILogger<ProviderRegistryWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderOrchestrator _orchestrator;
    private readonly IProviderRepository _repository;

    public ProviderRegistryWorker(
        ILogger<ProviderRegistryWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderOrchestrator orchestrator,
        IProviderRepository repository)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _orchestrator = orchestrator;
        _repository = repository;
    }

    // Ovwerride StartAsync to perform cleanup before starting
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.CleanupOrphanedContainersAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    // Override StopAsync to stop all providers gracefully
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.StopAllAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();
        SubscribeToEvents();
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("provider.commands");
        _messagingInfrastructure.BindQueue("provider.commands", Exchanges.Provider, ProviderRoutingKeys.StartAll);
    }

    private void SubscribeToEvents()
    {
        _messageSubscriber.Subscribe<StartAllProvidersCommand>("provider.commands", HandleStartAllProviders);
    }

    private async void HandleStartAllProviders(StartAllProvidersCommand command)
    {
        try
        {
            var providers = await _repository.GetAllAsync();

            if (!providers.Any())
            {
                PublishProvidersReady();
                return;
            }

            foreach (var provider in providers)
            {
                if (!provider.Enabled)
                {
                    continue;
                }

                try
                {
                    await _orchestrator.StartAsync(provider);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to start provider {provider.Id}");
                }
            }

            PublishProvidersReady();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start providers");
        }
    }

    private void PublishProvidersReady()
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllReady, new AllProvidersReadyEvent());
    }
}
