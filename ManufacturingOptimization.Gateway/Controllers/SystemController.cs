using ManufacturingOptimization.Common.Messaging.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/system")]
    public class SystemController : ControllerBase
    {
        private readonly ISystemReadinessService _readinessService;

        public SystemController(ISystemReadinessService readinessService)
        {
            _readinessService = readinessService;
        }

        /// <summary>
        /// Check if the system is started and ready
        /// </summary>
        [HttpGet("ready")]
        public IActionResult IsSystemReady()
        {
            var isReady = _readinessService.IsSystemReady && _readinessService.IsProvidersReady;
            return Ok(new { ready = isReady });
        }
    }
}
