using AutoMapper;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/providers")]
public class ProviderController : ControllerBase
{
    private readonly IProviderRepository _providerRegistry;
    private readonly ILogger<ProviderController> _logger;
    private readonly IMapper _mapper;

    public ProviderController(
        IProviderRepository providerRegistry,
        ILogger<ProviderController> logger,
        IMapper mapper)
    {
        _providerRegistry = providerRegistry;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Get list of all registered providers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProvidersResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProviders()
    {
        var providers = await _providerRegistry.GetAllAsync();
        var providerList = providers.ToList();
        var providerModels = _mapper.Map<List<Provider>>(providerList);

        return Ok(new ProvidersResponse
        {
            TotalProviders = providerList.Count,
            Providers = providerModels
        });
    }
}