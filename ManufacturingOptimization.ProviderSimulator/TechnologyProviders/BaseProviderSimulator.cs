using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.TechnologyProviders;

/// <summary>
/// Base class for all provider simulators with common estimation logic.
/// </summary>
public abstract class BaseProviderSimulator : IProviderSimulator
{
    protected readonly ILogger _logger;
    protected readonly Random _random = new();
    protected readonly Dictionary<ProcessType, double> _standardDurations;

    public ProviderModel Provider { get; protected set; } = new();

    protected BaseProviderSimulator(
        ILogger logger, 
        Dictionary<ProcessType, double> standardDurations)
    {
        _logger = logger;
        _standardDurations = standardDurations;
    }


    public ProposalModel HandleProposal(ProposeProcessToProviderCommand proposal)
    {
        var proposalModel = new ProposalModel
        {
            RequestId = proposal.RequestId,
            ProviderId = Provider.Id,
            Process = proposal.Process,
            ArrivedAt = DateTime.UtcNow,
            MotorSpecs = proposal.MotorSpecs
        };

        // Get process capability using normalized process name
        var processCapability = Provider.ProcessCapabilities.FirstOrDefault(pc => pc.Process == proposal.Process);
        
        if (processCapability == null)
        {
            proposalModel.Status = ProposalStatus.Declined;
            proposalModel.DeclineReason = $"{Provider.Name} does not have capability for {proposal.Process}";
            proposalModel.ModifiedAt = DateTime.UtcNow;

            return proposalModel;
        }

        // Accept proposal and generate estimate
        proposalModel.Status = ProposalStatus.Accepted;
        proposalModel.Estimate = GenerateEstimate(proposal.Process, processCapability);
        proposalModel.ModifiedAt = DateTime.UtcNow;

        return proposalModel;
    }

    public void HandleConfirmation(ProposalEntity proposalEntity)
    {
        // In a real implementation, update internal state, schedule resources, etc.
    }

    private ProcessEstimateModel GenerateEstimate(ProcessType process, ProcessCapabilityModel capability)
    {
        // Normalize activity name using ProcessType
        double baseHours;
        if (!_standardDurations.TryGetValue(process, out baseHours))
        {
            baseHours = 8.0;
        }

        // Apply provider's speed multiplier and add randomness ±30%
        var timeVariance = baseHours * 0.3;
        var actualHours = (baseHours * capability.SpeedMultiplier) + (_random.NextDouble() * 2 - 1) * timeVariance;

        // Calculate cost: CostPerHour * ActualHours, with ±20% variance
        var baseCost = capability.CostPerHour * (decimal)actualHours;
        var costVariance = baseCost * 0.2m;
        var actualCost = baseCost + (decimal)(_random.NextDouble() * 2 - 1) * costVariance;

        // Calculate emissions: Energy (kWh/h) * Hours * Carbon Intensity (kgCO2/kWh)
        var emissions = capability.EnergyConsumptionKwhPerHour 
            * actualHours 
            * capability.CarbonIntensityKgCO2PerKwh;

        return new ProcessEstimateModel
        {
            Cost = actualCost,
            Duration = TimeSpan.FromHours(actualHours),
            QualityScore = capability.QualityScore,
            EmissionsKgCO2 = emissions
        };
    }
}
