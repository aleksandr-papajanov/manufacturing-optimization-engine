using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.DTOs;
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
        [ProducesResponseType(typeof(OptimizationRequestAcceptedResponseDto), StatusCodes.Status202Accepted)]
        public IActionResult RequestOptimizationPlan([FromBody] OptimizationRequestDto request)
        {
            var requestModel = _mapper.Map<OptimizationRequestModel>(request);
            var command = new RequestOptimizationPlanCommand
            {
                Request = requestModel
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            return Accepted(new OptimizationRequestAcceptedResponseDto
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
        [ProducesResponseType(typeof(StrategySelectionResponseDto), StatusCodes.Status200OK)]
        public IActionResult SelectStrategy([FromBody] SelectOptimizationStrategyRequestDto request)
        {
            var command = new SelectStrategyCommand
            {
                RequestId = request.RequestId,
                SelectedStrategyId = request.SelectedStrategyId
            };
            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{command.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);

            return Ok(new StrategySelectionResponseDto
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
        [ProducesResponseType(typeof(StrategiesResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStrategies(Guid requestId)
        {
            var strategies = await _strategyRepository.GetForRequesttAsync(requestId);
            
            if (strategies != null && strategies.Any())
            {
                var strategyModels = _mapper.Map<List<OptimizationStrategyModel>>(strategies);
                
                return Ok(new StrategiesResponseDto
                {
                    IsReady = true,
                    Strategies = strategyModels,
                    Status = "Ready"
                });
            }

            return Ok(new StrategiesResponseDto
            {
                IsReady = false,
                Strategies = new List<OptimizationStrategyModel>(),
                Status = "Processing"
            });
        }

        /// <summary>
        /// Get optimization plan by request ID
        /// </summary>
        [HttpGet("plan/{requestId}")]
        [ProducesResponseType(typeof(OptimizationPlanModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlan(Guid requestId)
        {
            var planEntity = await _planRepository.GetByRequestIdAsync(requestId);

            if (planEntity == null)
            {
                return NotFound(new ErrorResponseDto { Message = $"No optimization plan found for request {requestId}" });
            }

            var planModel = _mapper.Map<OptimizationPlanModel>(planEntity);
            var planDto = _mapper.Map<OptimizationPlanDto>(planModel);
            return Ok(planDto);
        }
    }
}