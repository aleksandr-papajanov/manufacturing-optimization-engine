using System.Collections.Concurrent;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class InMemoryRequestResponseRepository : IRequestResponseRepository
{
    private readonly ConcurrentDictionary<Guid, IEvent?> _responses = new();
    private readonly ILogger<InMemoryRequestResponseRepository> _logger;

    public InMemoryRequestResponseRepository(ILogger<InMemoryRequestResponseRepository> logger)
    {
        _logger = logger;
    }

    public void AddRequest(ICommand command)
    {
        _responses[command.CommandId] = null;
    }

    public void AddResponse(IEvent response)
    {
        _responses[response.CommandId] = response;
    }

    public IEvent? GetByCommandId(Guid commandId)
    {
        return _responses.TryGetValue(commandId, out var e) ? e : null;
    }
}