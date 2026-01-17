using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Engine;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Services;
using ManufacturingOptimization.Engine.Services.Pipeline;
using ManufacturingOptimization.Engine.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<ProviderValidationSettings>(builder.Configuration.GetSection(ProviderValidationSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Provider registry
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();

// Pipeline factory
builder.Services.AddSingleton<IPipelineFactory, PipelineFactory>();

// Startup coordination
builder.Services.AddHostedService<StartupCoordinator>();

builder.Services.AddHostedService<ProviderCapabilityValidationService>();
builder.Services.AddHostedService<EngineWorker>();

// Register the Brain
builder.Services.AddSingleton<IRecommendationEngine, RecommendationEngine>();

var host = builder.Build();
host.Run();
