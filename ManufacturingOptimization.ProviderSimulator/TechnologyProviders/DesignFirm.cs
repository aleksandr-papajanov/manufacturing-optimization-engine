using ManufacturingOptimization.Common.Models;
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
        IOptions<ProcessStandardsSettings> processStandards,
        IProposalRepository proposalRepository)
        : base(logger, processStandards.Value.StandardDurationHours, proposalRepository)
    {
        var config = settings.Value;
        
        Provider = new Provider
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
