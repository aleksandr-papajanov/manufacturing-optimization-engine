using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class MachineShop : BaseProviderSimulator
{
    public MachineShop(
        ILogger<MachineShop> logger,
        IOptions<MachineShopSettings> settings,
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
