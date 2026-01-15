using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace TechnologyProvider.Simulator.TechnologyProviders;

public class PrecisionMachineShop : IProviderSimulator
{
    private readonly ILogger<PrecisionMachineShop> _logger;
    private readonly Random _random = new();
    private readonly PrecisionMachineShopSettings _settings;

    public Guid ProviderId { get; }
    public string ProviderName { get; }
    public List<string> Capabilities { get; }
    public double AxisHeight { get; }
    public double Power { get; }
    public double Tolerance { get; }

    public PrecisionMachineShop(
        ILogger<PrecisionMachineShop> logger,
        IOptions<PrecisionMachineShopSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        
        ProviderId = Guid.Parse(_settings.ProviderId);
        ProviderName = _settings.ProviderName;
        Capabilities = _settings.Capabilities;
        AxisHeight = _settings.AxisHeight;
        Power = _settings.Power;
        Tolerance = _settings.Tolerance;
    }

    public bool HandleProposal(ProposeProcessCommand proposal)
    {
        var accepted = _random.Next(0, 2) == 1;
      
        return accepted;
    }
}
