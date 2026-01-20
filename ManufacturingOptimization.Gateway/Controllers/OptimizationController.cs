using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
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
        public ActionResult<OptimizationRequestResponse> RequestOptimizationPlan([FromBody] OptimizationRequestDto requestDto)
        {
            // Map DTO to domain model
            var motorRequest = _mapper.Map<OptimizationRequest>(requestDto);
            
            var command = new RequestOptimizationPlanCommand
            {
                Request = motorRequest
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            var response = new OptimizationRequestResponse
            {
                Status = "Request accepted",
                CommandId = command.CommandId,
                RequestId = motorRequest.RequestId,
                Message = "Your optimization request is being processed. Multiple strategies will be generated."
            };

            return Accepted(response);
        }

        /// <summary>
        /// Select preferred optimization strategy
        /// </summary>
        [HttpPost("select")]
        [ProducesResponseType(typeof(StrategySelectionResponse), StatusCodes.Status200OK)]
        public ActionResult<StrategySelectionResponse> SelectStrategy([FromBody] SelectStrategyDto selection)
        {
            _logger.LogInformation(
                "Received strategy selection for Request {RequestId}: Strategy '{StrategyName}' (ID: {StrategyId})",
                selection.RequestId, selection.StrategyName, selection.StrategyId);

            var command = new SelectStrategyCommand
            {
                RequestId = selection.RequestId,
                SelectedStrategyId = selection.StrategyId,
                SelectedStrategyName = selection.StrategyName
            };

            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{selection.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);

            var response = new StrategySelectionResponse
            {
                Status = "Strategy selection received",
                RequestId = selection.RequestId,
                StrategyId = selection.StrategyId
            };

            return Ok(response);
        }

        /// <summary>
        /// Get available optimization strategies for a request
        /// </summary>
        [HttpGet("strategies/{requestId}")]
        [ProducesResponseType(typeof(StrategiesResponse), StatusCodes.Status200OK)]
        public ActionResult<StrategiesResponse> GetStrategies(Guid requestId)
        {
            var strategies = _strategyRepository.GetStrategies(requestId);
            
            if (strategies != null && strategies.Any())
            {
                // Map domain models to DTOs
                var strategyDtos = _mapper.Map<List<OptimizationStrategyDto>>(strategies);
                
                return Ok(new StrategiesResponse
                {
                    IsReady = true,
                    Strategies = strategyDtos,
                    Status = "Ready"
                });
            }

            return Ok(new StrategiesResponse
            {
                IsReady = false,
                Strategies = null,
                Status = "Processing"
            });
        }

        /// <summary>
        /// Get optimization plan by request ID
        /// </summary>
        [HttpGet("plan/{requestId}")]
        [ProducesResponseType(typeof(OptimizationPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<OptimizationPlanDto> GetPlan(Guid requestId)
        {
            var plan = _planRepository.GetByRequestId(requestId);

            if (plan == null)
            {
                return NotFound(new { Message = $"No optimization plan found for request {requestId}" });
            }

            var planDto = _mapper.Map<OptimizationPlanDto>(plan);
            return Ok(planDto);
        }
    }
}