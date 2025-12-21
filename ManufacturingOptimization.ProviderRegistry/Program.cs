using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.ProviderRegistry;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Services;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Configure OrchestrationSettings
builder.Services.Configure<OrchestrationSettings>(builder.Configuration.GetSection(OrchestrationSettings.SectionName));

// Configure DockerSettings
builder.Services.Configure<DockerSettings>(builder.Configuration.GetSection(DockerSettings.SectionName));

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Provider orchestration services
builder.Services.AddSingleton<IProviderRepository, JsonProviderRepository>();

// Register appropriate orchestrator based on mode
var orchestrationMode = builder.Configuration["Orchestration:Mode"] ?? "Production";
if (orchestrationMode == "Production")
{
    builder.Services.AddSingleton<IProviderOrchestrator, DockerProviderOrchestrator>();
}
else
{
    builder.Services.AddSingleton<IProviderOrchestrator, ComposeManagedOrchestrator>();
}

// Worker
builder.Services.AddHostedService<ProviderRegistryWorker>();

var host = builder.Build();
host.Run();
