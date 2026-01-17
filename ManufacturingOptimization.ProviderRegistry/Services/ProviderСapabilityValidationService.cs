using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Entities;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.ProviderRegistry.Services;
    
/// <summary>
/// Coordinates provider validation process.
/// Tracks validation requests/responses and publishes AllProvidersReady when done.
/// </summary>
public class ProviderСapabilityValidationService : BackgroundService
{
    private readonly ILogger<ProviderСapabilityValidationService> _logger;
    private readonly IMessagePublisher _publisher;
    private readonly IMessageSubscriber _subscriber;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IProviderOrchestrator _orchestrator;
    private readonly IProviderRepository _repository;
    
    private readonly ConcurrentDictionary<Guid, ProviderValidationState> _validations = new();

    public ProviderСapabilityValidationService(
        ILogger<ProviderСapabilityValidationService> logger,
        IMessagePublisher publisher,
        IMessageSubscriber subscriber,
        IMessagingInfrastructure messagingInfrastructure,
        IProviderOrchestrator orchestrator,
        IProviderRepository repository)
    {
        _logger = logger;
        _publisher = publisher;
        _subscriber = subscriber;
        _messagingInfrastructure = messagingInfrastructure;
        _orchestrator = orchestrator;
        _repository = repository;

        // Setup RabbitMQ subscriptions immediately in constructor
        // This ensures handlers are ready before any validation requests are sent
        SetupRabbitMq();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Single queue for both approved and declined responses
        _messagingInfrastructure.DeclareQueue("provider.validation.responses");
        
        // Clear any stale messages from previous runs
        _messagingInfrastructure.PurgeQueue("provider.validation.responses");
        
        _messagingInfrastructure.BindQueue("provider.validation.responses", Exchanges.Provider, ProviderRoutingKeys.ValidationApproved);
        _messagingInfrastructure.BindQueue("provider.validation.responses", Exchanges.Provider, ProviderRoutingKeys.ValidationDeclined);
        
        _subscriber.Subscribe<ProviderCapabilityValidationApprovedEvent>("provider.validation.responses", async response => await HandleValidationApprovedAsync(response));
        _subscriber.Subscribe<ProviderCapabilityValidationDeclinedEvent>("provider.validation.responses", HandleValidationDeclined);
    }

    public async Task StartValidationForAllProvidersAsync()
    {
        var providers = await _repository.GetAllAsync();

        if (!providers.Any())
        {
            _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllReady, new AllProvidersReadyEvent());
            return;
        }

        // Register all providers in pending validations first
        foreach (var provider in providers)
        {
            if (!provider.Enabled)
            {
                continue;
            }

            var state = new ProviderValidationState
            {
                Provider = provider,
                RequestedAt = DateTime.UtcNow,
                Status = ValidationStatus.Pending
            };

            _validations[provider.Id] = state;
        }

        // Send validation requests for all
        foreach (var state in _validations.Values)
        {
            var validationRequest = new ValidateProviderCapabilityCommand
            {
                ProviderId = state.Provider.Id,
                ProviderType = state.Provider.Type,
                ProviderName = state.Provider.Name,
                Capabilities = state.Provider.Capabilities,
                TechnicalCapabilities = state.Provider.TechnicalCapabilities
            };

            _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ValidationRequested, validationRequest);
        }
    }

    private async Task HandleValidationApprovedAsync(ProviderCapabilityValidationApprovedEvent response)
    {
        if (!_validations.TryGetValue(response.ProviderId, out var state))
        {
            return;
        }

        state.Status = ValidationStatus.Approved;
        state.RespondedAt = DateTime.UtcNow;

        try
        {
            await _orchestrator.StartAsync(state.Provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start provider {ProviderId}", response.ProviderId);
        }

        CheckIfAllValidationsComplete();
    }

    private void HandleValidationDeclined(ProviderCapabilityValidationDeclinedEvent response)
    {
        if (!_validations.TryGetValue(response.ProviderId, out var state))
        {
            return;
        }

        state.Status = ValidationStatus.Declined;
        state.RespondedAt = DateTime.UtcNow;

        _logger.LogWarning("Provider {ProviderId} ({ProviderType}) validation Declined: {Reason}", 
            response.ProviderId, response.ProviderType, response.Reason);

        CheckIfAllValidationsComplete();
    }

    private void CheckIfAllValidationsComplete()
    {
        if (_validations.IsEmpty)
        {
            return;
        }

        var allResponded = _validations.Values.All(v => v.Status != ValidationStatus.Pending);

        if (allResponded)
        {
            _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllReady, new AllProvidersReadyEvent());
        }
    }

    private enum ValidationStatus
    {
        Pending,
        Approved,
        Declined
    }

    private class ProviderValidationState
    {
        public Provider Provider { get; set; } = null!;
        public DateTime RequestedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public ValidationStatus Status { get; set; }
    }
}
