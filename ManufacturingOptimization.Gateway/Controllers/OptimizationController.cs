using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/optimization")]
    public class OptimizationController : ControllerBase
    {
        private readonly ILogger<OptimizationController> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationStrategyRepository _strategyRepository;
        private readonly IOptimizationPlanRepository _planRepository;
        private readonly IMapper _mapper;

        public OptimizationController(
            ILogger<OptimizationController> logger,
            IMessagePublisher messagePublisher,
            IOptimizationStrategyRepository strategyRepository,
            IOptimizationPlanRepository planRepository,
            IMapper mapper)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
            _strategyRepository = strategyRepository;
            _planRepository = planRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Submit optimization request
        /// </summary>
        [HttpPost("request")]
        [ProducesResponseType(typeof(OptimizationRequestResponse), StatusCodes.Status202Accepted)]
        public IActionResult RequestOptimizationPlan([FromBody] OptimizationRequest request)
        {
            
            var command = new RequestOptimizationPlanCommand
            {
                Request = request
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            return Accepted(new OptimizationRequestResponse
            {
                Status = "Request accepted",
                CommandId = command.CommandId,
                RequestId = request.RequestId,
                Message = "Your optimization request is being processed. Multiple strategies will be generated."
            });
        }

        /// <summary>
        /// Select preferred optimization strategy
        /// </summary>
        [HttpPost("select")]
        [ProducesResponseType(typeof(StrategySelectionResponse), StatusCodes.Status200OK)]
        public IActionResult SelectStrategy([FromBody] SelectStrategyCommand command)
        {
            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{command.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);

            return Ok(new StrategySelectionResponse
            {
                Status = "Strategy selection received",
                RequestId = command.RequestId,
                StrategyId = command.SelectedStrategyId
            });
        }

        /// <summary>
        /// Get available optimization strategies for a request
        /// </summary>
        [HttpGet("strategies/{requestId}")]
        [ProducesResponseType(typeof(StrategiesResponse), StatusCodes.Status200OK)]
        public IActionResult GetStrategies(Guid requestId)
        {
            var strategies = _strategyRepository.GetStrategies(requestId);
            
            if (strategies != null && strategies.Any())
            {
                var strategyModels = _mapper.Map<List<OptimizationStrategy>>(strategies);
                
                return Ok(new StrategiesResponse
                {
                    IsReady = true,
                    Strategies = strategyModels,
                    Status = "Ready"
                });
            }

            return Ok(new StrategiesResponse
            {
                IsReady = false,
                Strategies = new List<OptimizationStrategy>(),
                Status = "Processing"
            });
        }

        /// <summary>
        /// Get optimization plan by request ID
        /// </summary>
        [HttpGet("plan/{requestId}")]
        [ProducesResponseType(typeof(OptimizationPlan), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlan(Guid requestId)
        {
            var planEntity = await _planRepository.GetByRequestIdAsync(requestId);

            if (planEntity == null)
            {
                return NotFound(new ErrorResponse { Message = $"No optimization plan found for request {requestId}" });
            }

            var plan = _mapper.Map<OptimizationPlan>(planEntity);
            return Ok(plan);
        }
    }
}