using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Handlers;

/// <summary>
/// Handles optimization plan requests by executing the workflow pipeline.
/// Registered as Scoped service - dependencies can be injected directly.
/// </summary>
public class OptimizationRequestHandler : IMessageHandler<RequestOptimizationPlanCommand>
{
    private readonly IWorkflowPipelineFactory _pipelineFactory;
    private readonly ISystemReadinessService _readinessService;
    private readonly ILogger<OptimizationRequestHandler> _logger;

    public OptimizationRequestHandler(
        IWorkflowPipelineFactory pipelineFactory,
        ISystemReadinessService readinessService,
        ILogger<OptimizationRequestHandler> logger)
    {
        _pipelineFactory = pipelineFactory;
        _readinessService = readinessService;
        _logger = logger;
    }

    public async Task HandleAsync(RequestOptimizationPlanCommand command)
    {
        // Wait for system to be ready before processing
        await _readinessService.WaitForSystemReadyAsync();
        await _readinessService.WaitForProvidersReadyAsync();

        var pipeline = _pipelineFactory.CreateWorkflowPipeline();
        var context = new WorkflowContext { Request = command.Request };

        await pipeline.ExecuteAsync(context);
    }
}
