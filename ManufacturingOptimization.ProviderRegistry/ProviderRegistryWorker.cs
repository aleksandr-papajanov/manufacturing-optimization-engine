using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Services;

namespace ManufacturingOptimization.ProviderRegistry;

public class ProviderRegistryWorker : BackgroundService
{
    private readonly ILogger<ProviderRegistryWorker> _logger;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IProviderOrchestrator _orchestrator;
    private readonly ProviderСapabilityValidationService _validationCoordinator;

    public ProviderRegistryWorker(
        ILogger<ProviderRegistryWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IProviderRepository providerRepository,
        IProviderOrchestrator orchestrator,
        ProviderСapabilityValidationService validationCoordinator)
    {
        _logger = logger;
        _providerRepository = providerRepository;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _orchestrator = orchestrator;
        _validationCoordinator = validationCoordinator;

        SetupRabbitMq();
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
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        
        // Commands queue
        _messagingInfrastructure.DeclareQueue("provider.commands");
        _messagingInfrastructure.BindQueue("provider.commands", Exchanges.Provider, ProviderRoutingKeys.StartAll);
        _messageSubscriber.Subscribe<StartAllProvidersCommand>("provider.commands", HandleStartAllProviders);
    }

    private async void HandleStartAllProviders(StartAllProvidersCommand command)
    {
        await _validationCoordinator.StartValidationForAllProvidersAsync();
    }
}
