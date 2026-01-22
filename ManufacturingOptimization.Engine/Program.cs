using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Engine;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Data;
using ManufacturingOptimization.Engine.Data.Repositories;
using ManufacturingOptimization.Engine.Services;
using ManufacturingOptimization.Engine.Services.Pipeline;
using ManufacturingOptimization.Engine.Settings;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
Directory.CreateDirectory(dataDir); // Ensure directory exists
var dbPath = Path.Combine(dataDir, "EngineDatabase.db");

builder.Services.AddDbContext<EngineDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

// Register repositories directly
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IOptimizationPlanRepository, OptimizationPlanRepository>();
builder.Services.AddScoped<IOptimizationStrategyRepository, OptimizationStrategyRepository>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<ProviderValidationSettings>(builder.Configuration.GetSection(ProviderValidationSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Engine");
builder.Services.AddSingleton<ISystemReadinessService, StartupCoordinator>();
builder.Services.AddHostedService(sp => (StartupCoordinator)sp.GetRequiredService<ISystemReadinessService>());

// Pipeline factory
builder.Services.AddSingleton<IWorkflowPipelineFactory, PipelineFactory>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

builder.Services.AddHostedService<ProviderCapabilityValidationService>();
builder.Services.AddHostedService<EngineWorker>();

var host = builder.Build();
host.Run();
