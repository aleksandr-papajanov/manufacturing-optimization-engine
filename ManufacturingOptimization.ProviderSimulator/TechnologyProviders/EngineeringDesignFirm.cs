using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class EngineeringDesignFirm : IProviderSimulator
{
    private readonly ILogger<EngineeringDesignFirm> _logger;
    private readonly Random _random = new();
    private readonly EngineeringDesignFirmSettings _settings;

    public Guid ProviderId { get; }
    public string ProviderName { get; }
    public List<ProcessCapability> ProcessCapabilities { get; }
    public TechnicalCapabilities TechnicalCapabilities { get; }


    public EngineeringDesignFirm(
        ILogger<EngineeringDesignFirm> logger,
        IOptions<EngineeringDesignFirmSettings> settings)
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
            "Redesign" => 2000,
            "Certification" => 1800,
            _ => 1500
        };

        var baseHours = request.Activity switch
        {
            "Redesign" => 40,
            "Certification" => 30,
            _ => 20
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
            QualityScore = 0.85 + _random.NextDouble() * 0.15, // 0.85 - 1.0 (engineering excellence)
            EmissionsKgCO2 = emissions,
            CommandId = request.CommandId,
            Notes = $"Engineering estimate from {ProviderName}"
        };

        return estimate;
    }
}
