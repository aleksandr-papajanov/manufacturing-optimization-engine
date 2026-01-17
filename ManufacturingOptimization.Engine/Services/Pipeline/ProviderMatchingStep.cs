using ManufacturingOptimization.Engine.Abstractions;
using Common.Models;
using ManufacturingOptimization.Engine.Models;

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
        foreach (var processStep in context.ProcessSteps)
        {
            // Find providers that can perform this process
            var providersWithCapabilities = _providerRepository.FindByProcess(processStep.RequiredCapability);
            
            // Filter by technical requirements
            var matchedProviders = providersWithCapabilities
                .Where(pc => MeetsTechnicalRequirements(pc.Provider, context.Request))
                .Select(pc => new MatchedProvider
                {
                    ProviderId = pc.Provider.Id,
                    ProviderName = pc.Provider.Name,
                    ProviderType = pc.Provider.Type
                    // Cost, time, quality, and emissions will be filled by EstimationStep
                })
                .ToList();

            processStep.MatchedProviders = matchedProviders;

            if (processStep.MatchedProviders.Count == 0)
            {
                context.Errors.Add($"No providers found for step {processStep.StepNumber} ({processStep.Activity}) requiring '{processStep.RequiredCapability}' capability with sufficient technical capabilities");
            }
        }

        return Task.CompletedTask;
    }

    private bool MeetsTechnicalRequirements(Provider provider, MotorRequest request)
    {
        // Provider must be able to handle the motor's power and size
        bool canHandlePower = provider.TechnicalCapabilities.Power == 0 || provider.TechnicalCapabilities.Power >= request.Specs.PowerKW;
        bool canHandleSize = provider.TechnicalCapabilities.AxisHeight == 0 || provider.TechnicalCapabilities.AxisHeight >= request.Specs.AxisHeightMM;
        
        return canHandlePower && canHandleSize;
    }
}
