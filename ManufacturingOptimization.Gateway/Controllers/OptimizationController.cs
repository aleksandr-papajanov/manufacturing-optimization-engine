using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/optimization")]
public class OptimizationController : ControllerBase
{
    private readonly IMessagePublisher _publisher;
    private readonly IRequestResponseRepository _repository;
    private readonly ILogger<OptimizationController> _logger;

    public OptimizationController(
        IMessagePublisher publisher,
        IRequestResponseRepository repository,
        ILogger<OptimizationController> logger)
    {
        _publisher = publisher;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Request optimization from Engine
    /// </summary>
    [HttpPost("request")]
    public IActionResult RequestOptimization()
    {
        var command = new RequestOptimizationPlanCommand();
        
        _repository.AddRequest(command);
        _publisher.Publish(Exchanges.Optimization, "optimization.request", command);
        
        return Accepted(new { commandId = command.CommandId });
    }

    /// <summary>
    /// Check status of optimization request by CommandId
    /// </summary>
    [HttpGet("status/{commandId}")]
    public IActionResult CheckStatus(Guid commandId)
    {
        var response = _repository.GetByCommandId(commandId);

        if (response == null)
        {
            return NotFound(new { status = "not_found" });
        }

        if (response is OptimizationPlanCreatedEvent planEvent)
        {
            return Ok(new
            {
                status = "completed",
                data = new
                {
                    providerId = planEvent.ProviderId,
                    response = planEvent.Response
                }
            });
        }

        return Ok(new { status = "processing" });
    }
}
