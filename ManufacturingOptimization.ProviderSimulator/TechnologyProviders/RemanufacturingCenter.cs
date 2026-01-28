using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using ManufacturingOptimization.ProviderSimulator.TechnologyProviders;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class RemanufacturingCenter : BaseProviderSimulator
{
    public RemanufacturingCenter(
        ILogger<RemanufacturingCenter> logger,
        IPlannedProcessRepository plannedProcessRepository,
        IOptions<ProviderSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards)
        : base(logger, plannedProcessRepository, processStandards.Value.StandardDurationHours, settings.Value.WorkingHours)
    {
        var config = settings.Value;
        
        // Generate random breaks for testing (2-4 breaks per day)
        var random = new Random(config.ProviderId.GetHashCode());
        _workingHours.GenerateRandomBreaks(random.Next(2, 5), random);
        
        Provider = new ProviderModel
        {
            Id = Guid.Parse(config.ProviderId),
            Type = "MainRemanufacturingCenter",
            Name = config.ProviderName,
            Enabled = true,
            ProcessCapabilities = config.ProcessCapabilities,
            TechnicalCapabilities = config.TechnicalCapabilities
        };
    }
}
