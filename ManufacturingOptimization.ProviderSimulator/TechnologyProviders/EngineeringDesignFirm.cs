using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class EngineeringDesignFirm : IProviderSimulator
{
    private readonly ILogger<EngineeringDesignFirm> _logger;
    private readonly Random _random = new();

    public Guid ProviderId { get; }
    public string ProviderName { get; }

    public EngineeringDesignFirm(
        ILogger<EngineeringDesignFirm> logger,
        IOptions<EngineeringDesignFirmSettings> settings)
    {
        _logger = logger;
        ProviderId = Guid.Parse(settings.Value.ProviderId);
        ProviderName = settings.Value.ProviderName;
    }

    public bool HandleProposal(ProposeProcessCommand proposal)
    {
        var accepted = _random.Next(0, 2) == 1;
        
        return accepted;
    }
}
