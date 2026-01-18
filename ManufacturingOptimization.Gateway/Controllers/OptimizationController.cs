using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PanManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using IMessagePublisher = ManufacturingOptimization.Common.Messaging.Abstractions.IMessagePublisher;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/optimization")]
    public partial class OptimizationController : ControllerBase
    {
        private readonly ILogger<OptimizationController> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationStrategyRepository _strategyRepository;

        public OptimizationController(
            ILogger<OptimizationController> logger,
            IMessagePublisher messagePublisher,
            IOptimizationStrategyRepository strategyRepository)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
            _strategyRepository = strategyRepository;
        }

        // [NEW] Submit Optimization Request with full MotorRequest
        [HttpPost("request")]
        public IActionResult RequestOptimizationPlan([FromBody] OptimizationRequest motorRequest)
        {
            // Create command with full MotorRequest
            var command = new RequestOptimizationPlanCommand
            {
                Request = motorRequest
            };

            // Publish to optimization engine
            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            return Accepted(new 
            { 
                status = "Request accepted", 
                commandId = command.CommandId,
                requestId = motorRequest.RequestId,
                message = "Your optimization request is being processed. Multiple strategies will be generated."
            });
        }

        // [US-06] Submit Initial Request (Legacy - for backward compatibility)
        [HttpPost("submit")] 
        public IActionResult SubmitRequest([FromBody] MotorRequestDto request)
        {
            _logger.LogInformation("Received request {RequestId}. Raw Power: {Power}", request.RequestId, request.Power);

            double powerValue = 0;
            if (!string.IsNullOrEmpty(request.Power))
            {
                string cleanedPower = Regex.Replace(request.Power, "[^0-9.,]", "").Replace(",", ".");
                double.TryParse(cleanedPower, NumberStyles.Any, CultureInfo.InvariantCulture, out powerValue);
            }

            var eventMessage = new global::ManufacturingOptimization.Common.Messaging.CustomerRequestSubmittedEvent
            {
                RequestId = request.RequestId,
                CustomerId = request.CustomerId,
                RequiredPowerKW = powerValue
            };

            _messagePublisher.Publish(Exchanges.Optimization, "optimization.request", eventMessage);
            
            return Accepted(new { status = "Request submitted", requestId = request.RequestId });
        }

        // NEW: [US-07-T4] Select Preferred Strategy
        [HttpPost("select")]
        public IActionResult SelectStrategy([FromBody] SelectStrategyDto selection)
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

            // Publish with request-specific routing key so only the waiting pipeline receives it
            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{selection.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);

            return Ok(new { 
                status = "Strategy selection received", 
                requestId = selection.RequestId,
                strategyId = selection.StrategyId
            });
        }

        // NEW: [US-07] Get Strategies for Request
        [HttpGet("strategies/{requestId}")]
        public IActionResult GetStrategies(Guid requestId)
        {
            var strategies = _strategyRepository.GetStrategies(requestId);
            
            if (strategies != null && strategies.Any())
            {
                return Ok(new { 
                    isReady = true, 
                    strategies = strategies,
                    status = "Ready"
                });
            }

            return Ok(new { 
                isReady = false, 
                strategies = (object?)null,
                status = "Processing"
            });
        }
    }
}