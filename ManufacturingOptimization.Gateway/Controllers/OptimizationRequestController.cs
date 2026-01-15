using Microsoft.AspNetCore.Mvc;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using System.Text.RegularExpressions;
using System.Globalization;

using IMessagePublisher = ManufacturingOptimization.Common.Messaging.Abstractions.IMessagePublisher;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/optimization")]
    public class OptimizationRequestController : ControllerBase
    {
        private readonly ILogger<OptimizationRequestController> _logger;
        private readonly IMessagePublisher _messagePublisher;

        public OptimizationRequestController(
            ILogger<OptimizationRequestController> logger,
            IMessagePublisher messagePublisher)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
        }

        // --- DTOs ---

        public class MotorRequestDto
        {
            public string RequestId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string Power { get; set; } = string.Empty;
            public string TargetEfficiency { get; set; } = string.Empty;
        }

        public class SelectStrategyDto
        {
            public Guid RequestId { get; set; }
            public string ProviderId { get; set; } = string.Empty;
            public string StrategyName { get; set; } = string.Empty;
        }

        // --- ENDPOINTS ---

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

            // Note: Ensure CustomerRequestSubmittedEvent exists in your namespace
            // If this fails, check the exact namespace in Common.Messaging
            var eventMessage = new global::ManufacturingOptimization.Common.Messaging.CustomerRequestSubmittedEvent
            {
                RequestId = request.RequestId,
                CustomerId = request.CustomerId,
                RequiredPowerKW = powerValue
            };

            // Using string "optimization.exchange" to be safe
            _messagePublisher.Publish("optimization.exchange", "optimization.request", eventMessage);
            
            _logger.LogInformation("✓ Forwarded to Engine: {Power} kW", powerValue);

            return Accepted(new { status = "Request submitted", requestId = request.RequestId });
        }

        [HttpPost("select")]
        public IActionResult SelectStrategy([FromBody] SelectStrategyDto selection)
        {
            _logger.LogInformation($"Received strategy selection for Request {selection.RequestId}: Provider {selection.ProviderId}");

            var command = new SelectStrategyCommand
            {
                RequestId = selection.RequestId,
                SelectedProviderId = selection.ProviderId,
                SelectedStrategyName = selection.StrategyName
            };

            // Publish using the correct Routing Key: "optimization.strategy.selected"
            // Analytics is listening for exactly this!
            _messagePublisher.Publish("optimization.exchange", "optimization.strategy.selected", command);

            return Ok(new { status = "Selection Received", requestId = selection.RequestId });
        }
    }
}