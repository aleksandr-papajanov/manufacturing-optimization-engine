using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using ManufacturingOptimization.ProviderSimulator.TechnologyProviders;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class DesignFirm : BaseProviderSimulator
{
    public DesignFirm(
        ILogger<DesignFirm> logger,
        IPlannedProcessRepository plannedProcessRepository,
        IOptions<ProviderSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards)
        : base(logger, plannedProcessRepository, processStandards.Value.StandardDurationHours, settings.Value.WorkingHours)
    {
        var config = settings.Value;
        
        // Generate random breaks for testing (1-3 breaks per day)
        var random = new Random(config.ProviderId.GetHashCode());
        _workingHours.GenerateRandomBreaks(random.Next(1, 4), random);

        
        Provider = new ProviderModel
        {
            Id = Guid.Parse(config.ProviderId),
            Type = "EngineeringDesignFirm",
            Name = config.ProviderName,
            Enabled = true,
            ProcessCapabilities = config.ProcessCapabilities,
            TechnicalCapabilities = config.TechnicalCapabilities
        };
    }
}
