using ManufacturingOptimization.Analytics;
using ManufacturingOptimization.Analytics.Services;
using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.AddSingleton<RabbitMqService>();

// Existing Subscriber
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// Allows us to Bind Queues manually
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
// --------------------

builder.Services.AddSingleton<IAnalyticsStore, InMemoryAnalyticsStore>();
builder.Services.AddHostedService<AnalyticsWorker>(); 

var app = builder.Build();
app.MapGet("/", () => "Analytics Service is Running 📊");
app.Run();