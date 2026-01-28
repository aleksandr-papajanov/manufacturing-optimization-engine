using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.ProviderSimulator;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Mappings;
using ManufacturingOptimization.ProviderSimulator.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Handlers;
using ManufacturingOptimization.ProviderSimulator.Services;
using ManufacturingOptimization.ProviderSimulator.Settings;
using TechnologyProvider.Simulator.TechnologyProviders;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IPlannedProcessRepository, PlanedProcessRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Database lifecycle management
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<MotorSpecificationsMappingProfile>();
    c.AddProfile<ProposalMappingProfile>();
    c.AddProfile<ProcessEstimateMappingProfile>();
});

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.Configure<ProcessStandardsSettings>(builder.Configuration.GetSection(ProcessStandardsSettings.SectionName));
builder.Services.Configure<ProviderSettings>(builder.Configuration.GetSection(ProviderSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProposeProcessToProviderCommand>, ProcessProposalHandler>();
builder.Services.AddScoped<IMessageHandler<ConfirmProcessProposalCommand>, ProcessConfirmationHandler>();
builder.Services.AddScoped<IMessageHandler<RequestProvidersRegistrationCommand>, ProviderRegistrationRequestHandler>();

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
