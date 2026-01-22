using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Models;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.TechnologyProviders;

/// <summary>
/// Base class for all provider simulators with common estimation logic.
/// </summary>
public abstract class BaseProviderSimulator : IProviderSimulator
{
    protected readonly ILogger _logger;
    protected readonly Random _random = new();
    protected readonly Dictionary<ProcessType, double> _standardDurations;
    private readonly IProposalRepository _proposalRepository;

    public ProviderModel Provider { get; protected set; } = new();

    protected BaseProviderSimulator(
        ILogger logger, 
        Dictionary<ProcessType, double> standardDurations,
        IProposalRepository proposalRepository)
    {
        _logger = logger;
        _standardDurations = standardDurations;
        _proposalRepository = proposalRepository;
    }


    public ProcessProposalEstimatedEvent HandleProposal(ProposeProcessToProviderCommand proposal)
    {
        // Get process capability using normalized process name
        var processCapability = Provider.ProcessCapabilities.FirstOrDefault(pc => pc.Process == proposal.Process);
        
        if (processCapability == null)
        {
            // Decline: Cannot perform this activity
            return new ProcessProposalEstimatedEvent
            {
                RequestId = proposal.RequestId,
                ProviderId = Provider.Id,
                Process = proposal.Process,
                IsAccepted = false,
                DeclineReason = $"{Provider.Name} does not have capability for {proposal.Process}",
                Notes = "Activity not in provider capabilities"
            };
        }

        // Simple acceptance logic - could be extended with more complex business rules
        // For now, accept all proposals where we have the capability
        var estimate = GenerateEstimate(proposal.Process, processCapability);

        // Save accepted proposal to repository
        var acceptedProposal = new Proposal
        {
            ProviderId = Provider.Id,
            RequestId = proposal.RequestId,
            Process = proposal.Process,
            AcceptedAt = DateTime.UtcNow,
            Status = ProposalStatus.Accepted,
            Estimate = estimate
        };
        _proposalRepository.Add(acceptedProposal);
        
        return new ProcessProposalEstimatedEvent
        {
            RequestId = proposal.RequestId,
            ProviderId = Provider.Id,
            Process = proposal.Process,
            IsAccepted = true,
            Estimate = estimate,
            Notes = $"Proposal accepted by {Provider.Name}"
        };
    }

    public ProcessProposalConfirmedEvent HandleConfirmation(ConfirmProcessProposalCommand confirmation)
    {
        var scheduledStartTime = DateTime.UtcNow.AddDays(1); // Mock: schedule for tomorrow
        
        // Find existing accepted proposal and update to confirmed
        var proposals = _proposalRepository.GetByProviderId(Provider.Id);
        var existingProposal = proposals.FirstOrDefault(p => 
            p.RequestId == confirmation.RequestId && 
            p.Process == confirmation.Process &&
            p.Status == ProposalStatus.Accepted);

        if (existingProposal != null)
        {
            // Update existing proposal to confirmed
            _proposalRepository.ConfirmProposal(
                existingProposal.ProposalId,
                confirmation.PlanId,
                DateTime.UtcNow,
                scheduledStartTime);
        }
        else
        {
            // If not found (shouldn't happen), create new confirmed proposal
            var newProposal = new Proposal
            {
                ProviderId = Provider.Id,
                RequestId = confirmation.RequestId,
                PlanId = confirmation.PlanId,
                Process = confirmation.Process,
                AcceptedAt = DateTime.UtcNow,
                ConfirmedAt = DateTime.UtcNow,
                ScheduledStartTime = scheduledStartTime,
                Status = ProposalStatus.Confirmed
            };
            _proposalRepository.Add(newProposal);
        }
        
        return new ProcessProposalConfirmedEvent
        {
            RequestId = confirmation.RequestId,
            ProviderId = Provider.Id,
            Process = confirmation.Process,
            PlanId = confirmation.PlanId,
            ScheduledStartTime = scheduledStartTime,
            Notes = $"Process confirmed and scheduled by {Provider.Name}"
        };
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
