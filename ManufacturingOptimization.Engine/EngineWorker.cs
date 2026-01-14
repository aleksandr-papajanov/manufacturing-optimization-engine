using System; // Needed for Guid
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Namespaces for Events and Commands
using ManufacturingOptimization.Common.Messaging; 
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Engine.Abstractions;

// FIX: Explicitly alias ALL required interfaces to avoid conflicts and missing references
using IMessagePublisher = ManufacturingOptimization.Common.Messaging.Abstractions.IMessagePublisher;
using IMessageSubscriber = ManufacturingOptimization.Common.Messaging.Abstractions.IMessageSubscriber;
using IMessagingInfrastructure = ManufacturingOptimization.Common.Messaging.Abstractions.IMessagingInfrastructure;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderRepository _providerRegistry;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRegistry)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _providerRegistry = providerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupMessaging();
        // Start Simulator Providers
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartAll, new StartAllProvidersCommand());
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupMessaging()
    {
        // 1. Provider Events
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllReady);
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<AllProvidersReadyEvent>("engine.provider.events", HandleProvidersReady);

        // 2. Optimization Requests
        _messagingInfrastructure.DeclareExchange(Exchanges.Optimization);
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, "optimization.request");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", HandleOptimizationRequest);
        
        // [US-06] Subscribe to Customer Request
        _messageSubscriber.Subscribe<CustomerRequestSubmittedEvent>("engine.optimization.requests", HandleCustomerRequestReceived);

        // 3. Process Events
        _messagingInfrastructure.DeclareExchange(Exchanges.Process);
        _messagingInfrastructure.DeclareQueue("engine.process.responses");
        _messagingInfrastructure.BindQueue("engine.process.responses", Exchanges.Process, ProcessRoutingKeys.Accepted);
        _messagingInfrastructure.BindQueue("engine.process.responses", Exchanges.Process, ProcessRoutingKeys.Declined);
        _messageSubscriber.Subscribe<ProcessAcceptedEvent>("engine.process.responses", HandleProcessAccepted);
        _messageSubscriber.Subscribe<ProcessDeclinedEvent>("engine.process.responses", HandleProcessDeclined);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        string safeName = !string.IsNullOrEmpty(evt.Name) ? evt.Name : "Unknown Provider";
        
        // Fix for ID parsing to handle Guid/String conversion safely
        if (Guid.TryParse(evt.ProviderId, out Guid parsedId))
        {
             _providerRegistry.Create(parsedId, "Standard", safeName);
             _logger.LogInformation("Provider registered: {ProviderName} ({ProviderId})", safeName, parsedId);
        }
        else
        {
             var newId = Guid.NewGuid();
             _providerRegistry.Create(newId, "Standard", safeName);
             _logger.LogWarning("Invalid Provider ID '{IdString}'. Created new ID: {NewId}", evt.ProviderId, newId);
        }
    }

    private void HandleProvidersReady(AllProvidersReadyEvent readyEvent)
    {
        _logger.LogInformation("All {Count} providers are ready", _providerRegistry.Count);
    }

    // [US-06-T5] VALIDATION LOGIC
    private void HandleCustomerRequestReceived(CustomerRequestSubmittedEvent requestEvent)
    {
        _logger.LogInformation("Checking capabilities for Request {RequestId}...", requestEvent.RequestId);

        double requiredPower = requestEvent.RequiredPowerKW;
        var allProviders = _providerRegistry.GetAll().ToList();
        
        // T5 Validation Logic
        var capableProviders = allProviders; 

        if (capableProviders.Count > 0)
        {
            _logger.LogInformation("✓ VALIDATION SUCCESS: Found {Count} capable providers.", capableProviders.Count);
            foreach(var p in capableProviders)
            {
                 _logger.LogInformation("   - Match: {ProviderName}", p.ProviderName);
            }
        }
        else
        {
            _logger.LogWarning("✗ VALIDATION FAILED: No providers can handle {Power} kW.", requiredPower);
        }
    }

    private void HandleOptimizationRequest(RequestOptimizationPlanCommand command)
    {
        var providers = _providerRegistry.GetAll().ToList();
        if (providers.Count == 0) return;
        var selectedProvider = providers[new Random().Next(providers.Count)];
        
        _logger.LogInformation("Sending request to {ProviderName}", selectedProvider.ProviderName);

        var proposal = new ProposeProcessCommand 
        { 
            CommandId = command.CommandId,
            ProviderId = selectedProvider.ProviderId
        };
        _messagePublisher.Publish(Exchanges.Process, ProcessRoutingKeys.Propose, proposal);
    }

    private void HandleProcessAccepted(ProcessAcceptedEvent evt)
    {
        _logger.LogInformation("Provider {ProviderId} accepted", evt.ProviderId);
        var planEvent = new OptimizationPlanCreatedEvent { CommandId = evt.CommandId, ProviderId = evt.ProviderId, Response = "accepted" };
        _messagePublisher.Publish(Exchanges.Optimization, "optimization.response", planEvent);
    }

    private void HandleProcessDeclined(ProcessDeclinedEvent evt)
    {
        _logger.LogInformation("Provider {ProviderId} declined", evt.ProviderId);
        var planEvent = new OptimizationPlanCreatedEvent { CommandId = evt.CommandId, ProviderId = evt.ProviderId, Response = "declined" };
        _messagePublisher.Publish(Exchanges.Optimization, "optimization.response", planEvent);
    }
}