using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace TechnologyProvider.Simulator.TechnologyProviders;

/// <summary>
/// Base class for all provider simulators with common estimation logic.
/// </summary>
public abstract class BaseProviderSimulator : IProviderSimulator
{
    protected readonly ILogger _logger;
    protected readonly Random _random = new();
    protected readonly Dictionary<string, double> _standardDurations;

    public Guid ProviderId { get; protected set; }
    public string ProviderName { get; protected set; } = string.Empty;
    public List<ProcessCapability> ProcessCapabilities { get; protected set; } = new();
    public TechnicalCapabilities TechnicalCapabilities { get; protected set; } = new();

    protected BaseProviderSimulator(ILogger logger, Dictionary<string, double> standardDurations)
    {
        _logger = logger;
        _standardDurations = standardDurations;
    }

    public ProcessEstimatedEvent HandleEstimateRequest(RequestProcessEstimateCommand request)
    {
        // Get process capability for this activity
        var processCapability = ProcessCapabilities.FirstOrDefault(pc => pc.ProcessName == request.Activity);
        
        if (processCapability == null)
        {
            return new ProcessEstimatedEvent
            {
                ProviderId = ProviderId,
                Activity = request.Activity,
                CostEstimate = 0,
                TimeEstimate = TimeSpan.Zero,
                QualityScore = 0,
                EmissionsKgCO2 = 0,
                CommandId = request.CommandId,
                Notes = $"ERROR: {ProviderName} cannot perform {request.Activity}"
            };
        }

        // Get standard duration from configuration
        if (!_standardDurations.TryGetValue(request.Activity, out var baseHours))
        {
            baseHours = 8.0;
        }

        // Apply provider's speed multiplier and add randomness ±30%
        var timeVariance = baseHours * 0.3;
        var actualHours = (baseHours * processCapability.SpeedMultiplier) + (_random.NextDouble() * 2 - 1) * timeVariance;

        // Calculate cost: CostPerHour * ActualHours, with ±20% variance
        var baseCost = processCapability.CostPerHour * (decimal)actualHours;
        var costVariance = baseCost * 0.2m;
        var actualCost = baseCost + (decimal)(_random.NextDouble() * 2 - 1) * costVariance;

        // Calculate emissions: Energy (kWh/h) * Hours * Carbon Intensity (kgCO2/kWh)
        var emissions = processCapability.EnergyConsumptionKwhPerHour 
            * actualHours 
            * processCapability.CarbonIntensityKgCO2PerKwh;

        var estimate = new ProcessEstimatedEvent
        {
            ProviderId = ProviderId,
            Activity = request.Activity,
            CostEstimate = actualCost,
            TimeEstimate = TimeSpan.FromHours(actualHours),
            QualityScore = processCapability.QualityScore,
            EmissionsKgCO2 = emissions,
            CommandId = request.CommandId,
            Notes = $"Estimate from {ProviderName}"
        };

        return estimate;
    }
}
