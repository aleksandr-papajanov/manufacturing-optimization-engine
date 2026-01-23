using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Mappings;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Data;
using ManufacturingOptimization.Gateway.Handlers;
using ManufacturingOptimization.Gateway.Middleware;
using ManufacturingOptimization.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IOptimizationPlanRepository, OptimizationPlanRepository>();
builder.Services.AddScoped<IOptimizationStrategyRepository, OptimizationStrategyRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// Add AutoMapper
// Add AutoMapper
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<ProviderMappingProfile>();
    c.AddProfile<OptimizationMappingProfile>();
    c.AddProfile<GatewayMappingProfile>();
});

// Configure RabbitMQ Settings from appsettings.json
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();

// Map Messaging Interfaces
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProviderRegisteredEvent>, ProviderRegisteredHandler>();
builder.Services.AddScoped<IMessageHandler<OptimizationPlanReadyEvent>, OptimizationPlanReadyHandler>();
builder.Services.AddScoped<IMessageHandler<MultipleStrategiesReadyEvent>, StrategiesReadyHandler>();

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Gateway");
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();
builder.Services.AddHostedService(sp => (SystemReadinessService)sp.GetRequiredService<ISystemReadinessService>());

// Add Background Worker
builder.Services.AddHostedService<GatewayWorker>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddScoped<IProviderService, ProviderService>();

var app = builder.Build();

// Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Add middleware
app.UseExceptionHandling();
app.UseSystemReadiness();

app.UseAuthorization();
app.MapControllers();

app.Run();