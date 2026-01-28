using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

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
