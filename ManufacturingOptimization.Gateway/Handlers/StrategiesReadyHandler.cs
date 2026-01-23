using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Handlers;

/// <summary>
/// Handles multiple strategies ready events by storing strategies for later plan selection.
/// </summary>
public class StrategiesReadyHandler : IMessageHandler<MultipleStrategiesReadyEvent>
{
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<StrategiesReadyHandler> _logger;

    public StrategiesReadyHandler(
        IOptimizationStrategyRepository strategyRepository,
        IMapper mapper,
        ILogger<StrategiesReadyHandler> logger)
    {
        _strategyRepository = strategyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(MultipleStrategiesReadyEvent evt)
    {
        // Map strategies and set RequestId for filtering
        var entities = _mapper.Map<List<OptimizationStrategyEntity>>(evt.Strategies);
        
        foreach (var entity in entities)
        {
            entity.RequestId = evt.RequestId;
            entity.PlanId = null; // No plan yet
        }
        
        await _strategyRepository.AddForRequestAsync(evt.RequestId, entities);
    }
}
