using Microsoft.AspNetCore.Mvc;
using Common.Messaging;
using Common.Models;
using System.Threading.Tasks;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/requests")] // Matches REST best practices
    public class OptimizationRequestController : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<OptimizationRequestController> _logger;

        // Inject the RabbitMQ publisher service
        public OptimizationRequestController(IMessagePublisher messagePublisher, ILogger<OptimizationRequestController> logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        /// <summary>
        /// US-06: Submit a new motor optimization request.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitRequest([FromBody] MotorRequest request)
        {
            // 1. Basic Validation (T5 - Partial)
            if (request.Specs == null)
            {
                return BadRequest("Motor Specifications are required.");
            }

            // 2. Log reception
            _logger.LogInformation("Received optimization request for Customer {CustomerId} (Req: {RequestId})", 
                request.CustomerId, request.RequestId);

            // 3. Create the Event
            var eventMessage = new CustomerRequestSubmittedEvent(request);

            // 4. Publish to RabbitMQ (T6 - Publish Event)
            await _messagePublisher.PublishAsync(eventMessage);

            // 5. Return 202 Accepted (Standard for async processing)
            return Accepted(new 
            { 
                Message = "Request received and queued for processing.", 
                RequestId = request.RequestId,
                Status = "Pending"
            });
        }
    }
}