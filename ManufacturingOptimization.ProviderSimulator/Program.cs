using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.ProviderSimulator;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Services;
using ManufacturingOptimization.ProviderSimulator.Settings;
using TechnologyProvider.Simulator.TechnologyProviders;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.Configure<ProcessStandardsSettings>(builder.Configuration.GetSection(ProcessStandardsSettings.SectionName));
builder.Services.Configure<ProviderSettings>(builder.Configuration.GetSection(ProviderSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Register repository
builder.Services.AddSingleton<IProposalRepository, InMemoryProposalRepository>();

// Determine which technology provider to use based on environment variable
var providerType = Environment.GetEnvironmentVariable("PROVIDER_TYPE");

// All providers use the same "Provider" configuration section
switch (providerType)
{
    case "MainRemanufacturingCenter":
        
        builder.Services.AddSingleton<IProviderSimulator, RemanufacturingCenter>();
        break;
    
    case "EngineeringDesignFirm":
        builder.Services.AddSingleton<IProviderSimulator, DesignFirm>();
        break;
    
    case "PrecisionMachineShop":
        builder.Services.AddSingleton<IProviderSimulator, MachineShop>();
        break;
    
    default:
        throw new InvalidOperationException($"Unknown provider type: {providerType}");
}

builder.Services.AddHostedService<ProviderSimulatorWorker>();

var host = builder.Build();

host.Run();
