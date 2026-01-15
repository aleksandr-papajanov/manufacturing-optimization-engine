using ManufacturingOptimization.Analytics; // This namespace now exists!
using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure RabbitMQ Settings
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

// 2. Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// 3. Register the Background Worker
builder.Services.AddHostedService<AnalyticsWorker>(); 

var app = builder.Build();

app.MapGet("/", () => "Analytics Service is Running 📊");

app.Run();