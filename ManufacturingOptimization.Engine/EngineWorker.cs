using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMapper _mapper;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IServiceProvider _serviceProvider;
    private readonly IWorkflowPipelineFactory _pipelineFactory;
    private readonly ISystemReadinessService _readinessService;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMapper mapper,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IServiceProvider serviceProvider,
        IWorkflowPipelineFactory pipelineFactory,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _mapper = mapper;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _serviceProvider = serviceProvider;
        _pipelineFactory = pipelineFactory;
        _readinessService = readinessService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        SetupRabbitMq();

        // Give services time to start and subscribe
        await Task.Delay(1000, cancellationToken);

        // Engine is ready
        var evt = new ServiceReadyEvent
        {
            ServiceName = "Engine"
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, evt);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private void SetupRabbitMq()
    {
        // Provider events
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllRegistered);
        _messagingInfrastructure.PurgeQueue("engine.provider.events");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);

        // Optimization requests
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested);
        _messagingInfrastructure.PurgeQueue("engine.optimization.requests");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", async evt => await HandleOptimizationRequestAsync(evt));
    }
    
    private async void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();

        var providerEntity = _mapper.Map<ProviderEntity>(evt.Provider);
        await repository.AddAsync(providerEntity);
        await repository.SaveChangesAsync();
    }

    private async Task HandleOptimizationRequestAsync(RequestOptimizationPlanCommand command)
    {
        // Wait for system to be ready before processing
        await _readinessService.WaitForSystemReadyAsync();
        await _readinessService.WaitForProvidersReadyAsync();

        var pipeline = _pipelineFactory.CreateWorkflowPipeline();
        var context = new WorkflowContext { Request = command.Request };

        await pipeline.ExecuteAsync(context);
    }
}