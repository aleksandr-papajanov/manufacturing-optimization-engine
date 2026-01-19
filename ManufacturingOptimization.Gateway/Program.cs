using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Middleware;
using ManufacturingOptimization.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure RabbitMQ Settings from appsettings.json
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();

// Map Messaging Interfaces
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Gateway");
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();
builder.Services.AddHostedService(sp => (SystemReadinessService)sp.GetRequiredService<ISystemReadinessService>());

// Register Repositories (Singleton for in-memory implementations)
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();
builder.Services.AddSingleton<IRequestResponseRepository, InMemoryRequestResponseRepository>();
builder.Services.AddSingleton<IOptimizationStrategyRepository, InMemoryOptimizationStrategyRepository>();

// Add Background Worker
builder.Services.AddHostedService<GatewayWorker>();

var app = builder.Build();

// Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Add system readiness middleware before authorization
app.UseSystemReadiness();

app.UseAuthorization();
app.MapControllers();

app.Run();