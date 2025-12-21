using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.ProviderSimulator;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using TechnologyProvider.Simulator.TechnologyProviders;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Determine which technology provider to use based on environment variable
var providerType = Environment.GetEnvironmentVariable("PROVIDER_TYPE");

// Configure provider settings based on selected provider type
switch (providerType)
{
    case "MainRemanufacturingCenter":
        builder.Services.Configure<MainRemanufacturingCenterSettings>(builder.Configuration.GetSection(MainRemanufacturingCenterSettings.SectionName));
        builder.Services.AddSingleton<IProviderSimulator, MainRemanufacturingCenter>();
        break;
    
    case "EngineeringDesignFirm":
        builder.Services.Configure<EngineeringDesignFirmSettings>(builder.Configuration.GetSection(EngineeringDesignFirmSettings.SectionName));
        builder.Services.AddSingleton<IProviderSimulator, EngineeringDesignFirm>();
        break;
    
    case "PrecisionMachineShop":
        builder.Services.Configure<PrecisionMachineShopSettings>(builder.Configuration.GetSection(PrecisionMachineShopSettings.SectionName));
        builder.Services.AddSingleton<IProviderSimulator, PrecisionMachineShop>();
        break;
    
    default:
        throw new InvalidOperationException($"Unknown provider type: {providerType}");
}

builder.Services.AddHostedService<ProviderSimulatorWorker>();

var host = builder.Build();

host.Run();
