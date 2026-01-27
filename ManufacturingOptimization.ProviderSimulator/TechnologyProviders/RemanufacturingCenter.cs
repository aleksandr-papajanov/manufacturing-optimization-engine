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
        IOptions<ProviderSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards)
        : base(logger, processStandards.Value.StandardDurationHours)
    {
        var config = settings.Value;
        
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
