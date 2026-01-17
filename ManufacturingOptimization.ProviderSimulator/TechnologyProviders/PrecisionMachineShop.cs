using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class PrecisionMachineShop : IProviderSimulator
{
    private readonly ILogger<PrecisionMachineShop> _logger;
    private readonly Random _random = new();
    private readonly PrecisionMachineShopSettings _settings;

    public Guid ProviderId { get; }
    public string ProviderName { get; }
    public List<ProcessCapability> ProcessCapabilities { get; }
    public TechnicalCapabilities TechnicalCapabilities { get; }

    public PrecisionMachineShop(
        ILogger<PrecisionMachineShop> logger,
        IOptions<PrecisionMachineShopSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        
        ProviderId = Guid.Parse(_settings.ProviderId);
        ProviderName = _settings.ProviderName;
        ProcessCapabilities = _settings.ProcessCapabilities;
        TechnicalCapabilities = _settings.TechnicalCapabilities;
    }

    public ProcessEstimatedEvent HandleEstimateRequest(RequestProcessEstimateCommand request)
    {
        var baseCost = request.Activity switch
        {
            "Turning" => 1500,
            "Grinding" => 1200,
            "PartSubstitution" => 1000,
            _ => 1000
        };

        var baseHours = request.Activity switch
        {
            "Turning" => 16,
            "Grinding" => 12,
            "PartSubstitution" => 6,
            _ => 8
        };

        // Add randomness ±20% for cost, ±30% for time
        var costVariance = baseCost * 0.2m;
        var timeVariance = baseHours * 0.3;
        var actualHours = baseHours + (_random.NextDouble() * 2 - 1) * timeVariance;
        
        // Get process capability for emissions calculation
        var processCapability = ProcessCapabilities.FirstOrDefault(pc => pc.ProcessName == request.Activity);

        var emissions = processCapability != null 
            ? processCapability.EnergyConsumptionKwhPerHour * actualHours * processCapability.CarbonIntensityKgCO2PerKwh
            : 0;
        
        var estimate = new ProcessEstimatedEvent
        {
            ProviderId = ProviderId,
            Activity = request.Activity,
            CostEstimate = baseCost + (decimal)(_random.NextDouble() * 2 - 1) * costVariance,
            TimeEstimate = TimeSpan.FromHours(actualHours),
            QualityScore = 0.8 + _random.NextDouble() * 0.2, // 0.8 - 1.0 (high precision)
            EmissionsKgCO2 = emissions,
            CommandId = request.CommandId,
            Notes = $"Precision estimate from {ProviderName}"
        };

        return estimate;
    }
}
