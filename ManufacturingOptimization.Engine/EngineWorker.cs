using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Engine.Abstractions;
using Common.Models; 
using ManufacturingOptimization.Engine.Models; 
using System.Text.Json;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IProviderRepository _providerRepository;
    private readonly IRecommendationEngine _recommendationEngine;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessageSubscriber messageSubscriber,
        IProviderRepository providerRepository,
        IRecommendationEngine recommendationEngine)
    {
        _logger = logger;
        _messageSubscriber = messageSubscriber;
        _providerRepository = providerRepository;
        _recommendationEngine = recommendationEngine;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Engine Worker started. Waiting for requests...");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", HandleOptimizationRequest);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void HandleOptimizationRequest(RequestOptimizationPlanCommand command)
    {
        _logger.LogInformation($"Processing Request {command.CommandId}...");

        // 1. Reconstruct MotorRequest 
        var motorRequest = new MotorRequest
        {
            RequestId = command.CommandId,
            Constraints = new RequestConstraints 
            { 
                Priority = OptimizationPriority.HighestQuality 
            }
        };

        // 2. Fetch from Repo
        var registeredProviders = _providerRepository.GetAll();

        // --- FALLBACK FOR TESTING (If DB is empty after restart) ---
        if (!registeredProviders.Any())
        {
            _logger.LogWarning("⚠️ Repo is empty! Injecting MOCK providers for ranking test.");
            registeredProviders = new List<RegisteredProvider>
            {
                new RegisteredProvider { ProviderId = Guid.NewGuid(), ProviderName = "Fast & Expensive Corp" },
                new RegisteredProvider { ProviderId = Guid.NewGuid(), ProviderName = "Eco Green Motors" },
                new RegisteredProvider { ProviderId = Guid.NewGuid(), ProviderName = "Budget Fixers Ltd" }
            };
        }
        // -----------------------------------------------------------

        var allProviders = registeredProviders.Select(rp => new Provider 
        {
            Id = rp.ProviderId.ToString(),
            Name = rp.ProviderName,
            Capabilities = new Capabilities 
            { 
                MaxPowerKW = 100, 
                SupportedTypes = new List<string> { "IE1", "IE2", "IE3", "IE4" } 
            } 
        }).ToList();

        // 3. Filter Capable Providers
        var capableProviders = allProviders
            .Where(p => p.Capabilities.MaxPowerKW >= 5.5 && p.Capabilities.SupportedTypes.Contains("IE1"))
            .ToList();

        if (!capableProviders.Any())
        {
            _logger.LogWarning($"✗ No capable providers found for Request {command.CommandId}.");
            return;
        }

        _logger.LogInformation($"✓ VALIDATION SUCCESS: Found {capableProviders.Count} capable providers.");

        // 4. Ask the Brain to Rank them
        var recommendations = _recommendationEngine.GenerateRecommendations(motorRequest, capableProviders);
        
        // 5. Log the Results
        _logger.LogInformation("--- OPTIMIZATION RESULTS ---");
        foreach (var rec in recommendations)
        {
            _logger.LogInformation($"Option: {rec.ProviderName} | Score: {rec.MatchScore:F1} | Cost: ${rec.EstimatedCost} | Time: {rec.EstimatedLeadTimeDays} days");
        }

        // 6. Select the Winner
        if (recommendations.Any())
        {
            var winner = recommendations.First();
            _logger.LogInformation($"🏆 WINNER SELECTED: {winner.ProviderName} (Score: {winner.MatchScore:F1})");
        }
    }
}