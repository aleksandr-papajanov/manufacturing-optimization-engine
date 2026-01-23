using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using ManufacturingOptimization.ProviderSimulator.TechnologyProviders;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class MachineShop : BaseProviderSimulator
{
    public MachineShop(
        ILogger<MachineShop> logger,
        IOptions<ProviderSettings> settings,
        IOptions<ProcessStandardsSettings> processStandards,
        IProposalRepository proposalRepository)
        : base(logger, processStandards.Value.StandardDurationHours, proposalRepository)
    {
        var config = settings.Value;
        
        Provider = new ProviderModel
        {
            Id = Guid.Parse(config.ProviderId),
            Type = "PrecisionMachineShop",
            Name = config.ProviderName,
            Enabled = true,
            ProcessCapabilities = config.ProcessCapabilities,
            TechnicalCapabilities = config.TechnicalCapabilities
        };
    }
}
