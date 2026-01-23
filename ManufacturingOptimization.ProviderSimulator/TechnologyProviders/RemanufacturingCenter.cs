using ManufacturingOptimization.Common.Models;
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
        IOptions<ProcessStandardsSettings> processStandards,
        IProposalRepository proposalRepository)
        : base(logger, processStandards.Value.StandardDurationHours, proposalRepository)
    {
        var config = settings.Value;
        
        Provider = new Provider
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
