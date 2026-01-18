using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// 2. Configure RabbitMQ Settings from appsettings.json
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// 3. Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();

// 4. Map Messaging Interfaces
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// 5. Register Repositories (Singleton for in-memory implementations)
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();
builder.Services.AddSingleton<IRequestResponseRepository, InMemoryRequestResponseRepository>();

// NEW: Register Strategy Cache Service
builder.Services.AddSingleton<StrategyCacheService>();

// 6. Add Background Worker
builder.Services.AddHostedService<GatewayWorker>();

var app = builder.Build();

// 7. Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();
app.MapControllers();

app.Run();