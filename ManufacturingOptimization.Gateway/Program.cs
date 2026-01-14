using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Configure RabbitMQ Settings using the Options Pattern
// The RabbitMqService constructor requires IOptions<RabbitMqSettings>, not the raw object.
builder.Services.Configure<RabbitMqSettings>(options =>
{
    options.Host = "rabbitmq";
    options.Port = 5672;
    options.Username = "admin";
    options.Password = "admin123";
});

// 3. Register the Service
builder.Services.AddSingleton<RabbitMqService>();

// 4. Map Interfaces to the Single Instance
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

var app = builder.Build();

// 5. Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

// Note: No explicit .Connect() needed; the service connects lazily on first use.

app.Run();