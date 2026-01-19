using AutoMapper;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
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
    /// Get list of all registered providers from in-memory registry
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ProvidersResponse), StatusCodes.Status200OK)]
    public ActionResult<ProvidersResponse> GetProviders()
    {
        var providers = _providerRegistry.GetAll();
        
        // Map domain models to DTOs
        var providerDtos = _mapper.Map<List<ProviderDto>>(providers);
        
        var response = new ProvidersResponse
        {
            TotalProviders = providerDtos.Count,
            Providers = providerDtos
        };

        return Ok(response);
    }
}