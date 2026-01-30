using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ManufacturingOptimization.ProviderRegistry.Services;

public class QuoteRequestHandler : IMessageHandler<ProviderQuoteRequest>
{
    private readonly IProviderRepository _repository;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<QuoteRequestHandler> _logger;

    public QuoteRequestHandler(IProviderRepository repository, IMessagePublisher publisher, ILogger<QuoteRequestHandler> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderQuoteRequest message)
    {
        _logger.LogInformation("Processing quote request for {ProcessName}...", message.ProcessName);

        // Simple logic: Find providers who can do the process and return a dummy quote
        var providers = await _repository.GetAllAsync();
        
        foreach (var provider in providers)
        {
             var reply = new ProviderQuoteResponse
             {
                 RequestId = message.RequestId,
                 ProviderId = provider.Id.ToString(),
                 ProviderName = provider.Name,
                 Price = new Random().Next(100, 1000),
                 EstimatedDuration = TimeSpan.FromHours(new Random().Next(2, 48))
             };

             _publisher.PublishReply(message, reply);
        }
    }
}