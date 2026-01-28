using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/optimization")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimizationService _optimizationService;

        public OptimizationController(IOptimizationService optimizationService)
        {
            _optimizationService = optimizationService;
        }

        /// <summary>
        /// Submit optimization request
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> RequestOptimizationPlan([FromBody] OptimizationRequestDto request)
        {
            var requestId = await _optimizationService.RequestOptimizationPlanAsync(request);
            return Accepted(requestId);
        }

        /// <summary>
        /// Select preferred optimization strategy
        /// </summary>
        [HttpPost("strategies/{requestId}/select/{strategyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SelectStrategy(Guid requestId, Guid strategyId)
        {
            await _optimizationService.SelectStrategyAsync(requestId, strategyId);
            return Ok();
        }

        /// <summary>
        /// Get available optimization strategies for a request
        /// </summary>
        [HttpGet("strategies/{requestId}")]
        [ProducesResponseType(typeof(List<OptimizationStrategyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStrategies(Guid requestId)
        {
            var response = await _optimizationService.GetStrategiesAsync(requestId);
            return Ok(response);
        }

        /// <summary>
        /// Get optimization plan by request ID
        /// </summary>
        [HttpGet("plan/{requestId}")]
        [ProducesResponseType(typeof(OptimizationPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlan(Guid requestId)
        {
            var planDto = await _optimizationService.GetPlanAsync(requestId);
            return Ok(planDto);
        }
    }
}