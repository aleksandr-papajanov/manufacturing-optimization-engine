using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// 1. Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// --- FIX: Register ALL Legacy Repositories ---
// 1. Provider Repository (For "Get Providers List")
builder.Services.AddScoped<IProviderRepository, InMemoryProviderRepository>();

// 2. Request/Response Repository (For "Run Random Demo")
builder.Services.AddScoped<IRequestResponseRepository, InMemoryRequestResponseRepository>(); 

// 3. Configure RabbitMQ Settings (For US-06)
builder.Services.Configure<RabbitMqSettings>(options =>
{
    options.Host = "rabbitmq";
    options.Port = 5672;
    options.Username = "admin";
    options.Password = "admin123";
});

// 4. Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();

// 5. Map Messaging Interfaces
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

var app = builder.Build();

// 6. Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();