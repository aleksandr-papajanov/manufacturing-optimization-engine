using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Requests cost, time, and quality estimates from each matched provider.
/// Uses RPC pattern (Request-Reply) to get estimates from providers.
/// </summary>
public class EstimationStep : IWorkflowStep
{
    private readonly ILogger<EstimationStep> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public EstimationStep(
        ILogger<EstimationStep> logger,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    public string Name => "Estimation";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        foreach (var step in context.ProcessSteps)
        {
            var estimateTasks = step.MatchedProviders.Select(async provider =>
            {
                try
                {
                    var request = new RequestProcessEstimateCommand
                    {
                        RequestId = context.Request.RequestId,
                        ProviderId = provider.ProviderId,
                        Activity = step.Activity,
                        MotorSpecs = context.Request.Specs
                    };

                    var response = await _messagePublisher.RequestReplyAsync<ProcessEstimatedEvent>(
                        Exchanges.Process,
                        $"process.estimate.{provider.ProviderId}",
                        request,
                        TimeSpan.FromSeconds(10));

                    if (response != null)
                    {
                        provider.CostEstimate = response.CostEstimate;
                        provider.TimeEstimate = response.TimeEstimate;
                        provider.QualityScore = response.QualityScore;
                        provider.EmissionsKgCO2 = response.EmissionsKgCO2;
                    }
                    else
                    {
                        _logger.LogWarning("Provider {ProviderId} did not respond to estimate request for {Activity}",
                            provider.ProviderId, step.Activity);
                        
                        // Use fallback estimates
                        provider.CostEstimate = 0;
                        provider.TimeEstimate = TimeSpan.Zero;
                        provider.QualityScore = 0;
                        provider.EmissionsKgCO2 = 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting estimate from provider {ProviderId}", provider.ProviderId);
                    
                    // Use fallback estimates
                    provider.CostEstimate = 0;
                    provider.TimeEstimate = TimeSpan.Zero;
                    provider.QualityScore = 0;
                    provider.EmissionsKgCO2 = 0;
                }
            });

            await Task.WhenAll(estimateTasks);
        }
    }
}
