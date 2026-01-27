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
        IOptions<ProviderSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards)
        : base(logger, processStandards.Value.StandardDurationHours)
    {
        var config = settings.Value;
        
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
