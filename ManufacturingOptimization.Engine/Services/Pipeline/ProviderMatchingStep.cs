using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Abstractions.Pipeline;
using Common.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Step 2: Provider Matching
/// For each process step, finds providers with the required capability.
/// </summary>
public class ProviderMatchingStep : IPipelineStep
{
    private readonly IProviderRepository _providerRepository;
    private readonly ILogger<ProviderMatchingStep> _logger;

    public string Name => "Provider Matching";

    public ProviderMatchingStep(
        IProviderRepository providerRepository,
        ILogger<ProviderMatchingStep> logger)
    {
        _providerRepository = providerRepository;
        _logger = logger;
    }

    public Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Matching providers for {StepCount} process steps", context.ProcessSteps.Count);

        foreach (var processStep in context.ProcessSteps)
        {
            // Find providers with required capability
            var providers = _providerRepository.FindByCapability(processStep.RequiredCapability);
            
            // Filter by technical requirements
            var matchedProviders = providers
                .Where(p => MeetsTechnicalRequirements(p, context.Request))
                .ToList();

            processStep.MatchedProviders = matchedProviders.Select(p => new MatchedProvider
            {
                ProviderId = p.ProviderId,
                ProviderName = p.ProviderName,
                ProviderType = p.ProviderType
            }).ToList();

            _logger.LogDebug("Step {StepNumber} ({Activity}): Found {ProviderCount}/{TotalCount} providers with '{Capability}' capability matching technical requirements",
                processStep.StepNumber, processStep.Activity, processStep.MatchedProviders.Count, providers.Count, processStep.RequiredCapability);

            if (processStep.MatchedProviders.Count == 0)
            {
                context.Errors.Add($"No providers found for step {processStep.StepNumber} ({processStep.Activity}) requiring '{processStep.RequiredCapability}' capability with sufficient technical capabilities");
            }
        }

        if (context.IsSuccess)
        {
            _logger.LogInformation("Provider matching completed. All steps have providers.");
        }

        return Task.CompletedTask;
    }

    private bool MeetsTechnicalRequirements(RegisteredProvider provider, MotorRequest request)
    {
        // Provider must be able to handle the motor's power and size
        bool canHandlePower = provider.Power == 0 || provider.Power >= request.Specs.PowerKW;
        bool canHandleSize = provider.AxisHeight == 0 || provider.AxisHeight >= request.Specs.AxisHeightMM;
        
        return canHandlePower && canHandleSize;
    }
}
