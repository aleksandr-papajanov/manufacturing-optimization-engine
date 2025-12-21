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
                providerId = p.ProviderId,
                providerType = p.ProviderType,
                providerName = p.ProviderName,
                registeredAt = p.RegisteredAt
            })
        });
    }
}