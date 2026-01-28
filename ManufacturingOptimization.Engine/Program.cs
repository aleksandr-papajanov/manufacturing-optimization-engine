using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Mappings;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.Engine;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Data;
using ManufacturingOptimization.Engine.Handlers;
using ManufacturingOptimization.Engine.Services;
using ManufacturingOptimization.Engine.Services.Pipeline;
using ManufacturingOptimization.Engine.Settings;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Engine.Services.Execution;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IOptimizationPlanRepository, OptimizationPlanRepository>();
builder.Services.AddScoped<IOptimizationStrategyRepository, OptimizationStrategyRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Add AutoMapper
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<ProviderMappingProfile>();
    c.AddProfile<OptimizationMappingProfile>();
});

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<ProviderValidationSettings>(builder.Configuration.GetSection(ProviderValidationSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Engine");
builder.Services.AddSingleton<ISystemReadinessService, StartupCoordinator>();
builder.Services.AddHostedService(sp => (StartupCoordinator)sp.GetRequiredService<ISystemReadinessService>());

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProviderRegisteredEvent>, ProviderRegisteredHandler>();
builder.Services.AddScoped<IMessageHandler<RequestOptimizationPlanCommand>, OptimizationRequestHandler>();

// Register the Plan Coordinator
builder.Services.AddSingleton<ManufacturingOptimization.Engine.Services.Execution.PlanCoordinator>();

// Pipeline factory
builder.Services.AddSingleton<IWorkflowPipelineFactory, PipelineFactory>();

builder.Services.AddHostedService<ProviderCapabilityValidationService>();
builder.Services.AddHostedService<EngineWorker>();

var host = builder.Build();

// -----------------------------------------------------------------------------
// Manual Subscription Registration for Execution Coordinator
// -----------------------------------------------------------------------------
var subscriber = host.Services.GetRequiredService<IMessageSubscriber>();
var coordinator = host.Services.GetRequiredService<ManufacturingOptimization.Engine.Services.Execution.PlanCoordinator>();

// 1. Trigger: Start Execution when Plan is Ready
await subscriber.SubscribeAsync<OptimizationPlanReadyEvent>(
    Exchanges.Optimization,
    OptimizationRoutingKeys.PlanReady,
    coordinator.HandleAsync,
    "engine.coordinator.plan_ready_queue"
);

// 2. Progression: Move to next step when Provider finishes
await subscriber.SubscribeAsync<ProcessExecutionCompletedEvent>(
    Exchanges.Process,
    ProcessRoutingKeys.ExecutionCompleted,
    coordinator.HandleAsync,
    "engine.coordinator.step_completion_queue"
);

host.Run();
