using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Engine.Handlers;

/// <summary>
/// Handles provider registration events by persisting provider data to the database.
/// Registered as Scoped service - dependencies can be injected directly.
/// </summary>
public class ProviderRegisteredHandler : IMessageHandler<ProviderRegisteredEvent>
{
    private readonly IProviderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderRegisteredHandler> _logger;

    public ProviderRegisteredHandler(
        IProviderRepository repository,
        IMapper mapper,
        ILogger<ProviderRegisteredHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderRegisteredEvent evt)
    {
        var providerEntity = _mapper.Map<ProviderEntity>(evt.Provider);
        await _repository.AddAsync(providerEntity);
        await _repository.SaveChangesAsync();
    }
}
