using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/providers")]
public class ProviderController : ControllerBase
{
    private readonly IProviderRepository _providerRegistry;
    private readonly ILogger<ProviderController> _logger;

    public ProviderController(
        IProviderRepository providerRegistry,
        ILogger<ProviderController> logger)
    {
        _providerRegistry = providerRegistry;
        _logger = logger;
    }

    /// <summary>
    /// Get list of all registered providers from in-memory registry
    /// </summary>
    [HttpGet]
    public IActionResult GetProviders()
    {
        var providers = _providerRegistry.GetAll();
        
        return Ok(new
        {
            totalProviders = providers.Count(),
            providers = providers.Select(p => new
            {
                providerId = p.Id,
                providerType = p.Type,
                providerName = p.Name,
                enabled = p.Enabled,
                processCapabilities = p.ProcessCapabilities.Select(pc => new 
                {
                    processName = pc.ProcessName,
                    costPerHour = pc.CostPerHour,
                    qualityScore = pc.QualityScore,
                    carbonIntensityKgCO2PerKwh = pc.CarbonIntensityKgCO2PerKwh,
                    usesRenewableEnergy = pc.UsesRenewableEnergy
                })
            })
        });
    }
}