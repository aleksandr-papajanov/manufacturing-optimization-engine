using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// 1. Register the concrete service
builder.Services.AddSingleton<RabbitMqService>();

// 2. Register the specific interface required by your Controller (US-06 fix)
// FIX: Use 'global::' to force the compiler to look at the ROOT namespace.
// This prevents it from getting confused with the current project's namespace.
builder.Services.AddSingleton<global::Common.Messaging.IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());

// 3. Register the other interfaces using fully qualified names to be safe
builder.Services.AddSingleton<ManufacturingOptimization.Common.Messaging.Abstractions.IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<ManufacturingOptimization.Common.Messaging.Abstractions.IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Add in-memory repository
builder.Services.AddSingleton<IRequestResponseRepository, InMemoryRequestResponseRepository>();
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();

// Add background worker
builder.Services.AddHostedService<GatewayWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();