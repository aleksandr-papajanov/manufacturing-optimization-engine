using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/providers")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProviderController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    /// <summary>
    /// Get list of all registered providers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProviders()
    {
        var response = await _providerService.GetProvidersAsync();
        return Ok(response);
    }
}