using Microsoft.AspNetCore.Mvc;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
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

        public class MotorRequestDto
        {
            public string RequestId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string Power { get; set; } = string.Empty;
            public string TargetEfficiency { get; set; } = string.Empty;
        }

        // FIX: Change route to "submit" to avoid conflict with the legacy controller
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
            
            _logger.LogInformation("✓ Forwarded to Engine: {Power} kW", powerValue);

            return Accepted(new { status = "Request submitted", requestId = request.RequestId });
        }
    }
}