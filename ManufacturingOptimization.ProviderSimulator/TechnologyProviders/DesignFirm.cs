using ManufacturingOptimization.ProviderSimulator.Settings;
using ManufacturingOptimization.ProviderSimulator.TechnologyProviders;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class DesignFirm : BaseProviderSimulator
{
    public DesignFirm(
        ILogger<DesignFirm> logger,
        IOptions<DesignFirmSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards)
        : base(logger, processStandards.Value.StandardDurationHours)
    {
        var config = settings.Value;
        
        ProviderId = Guid.Parse(config.ProviderId);
        ProviderName = config.ProviderName;
        ProcessCapabilities = config.ProcessCapabilities;
        TechnicalCapabilities = config.TechnicalCapabilities;
    }
}
