using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Settings;
using Microsoft.Extensions.Options;
using Common.Models;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// Handles provider validation requests from ProviderRegistry.
/// </summary>
public class ProviderCapabilityValidationService : BackgroundService
{
    private readonly ILogger<ProviderCapabilityValidationService> _logger;
    private readonly IMessageSubscriber _subscriber;
    private readonly IMessagePublisher _publisher;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly ProviderValidationSettings _settings;

    public ProviderCapabilityValidationService(
        ILogger<ProviderCapabilityValidationService> logger,
        IMessageSubscriber subscriber,
        IMessagePublisher publisher,
        IMessagingInfrastructure messagingInfrastructure,
        IProviderRepository providerRepository,
        IOptions<ProviderValidationSettings> settings)
    {
        _logger = logger;
        _subscriber = subscriber;
        _publisher = publisher;
        _messagingInfrastructure = messagingInfrastructure;
        _settings = settings.Value;

        // Setup RabbitMQ immediately in constructor to ensure queues are purged
        // and handlers are ready before any validation requests are sent
        SetupRabbitMq();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareQueue("engine.provider.validation");
        
        // Clear any stale validation requests from previous runs
        _messagingInfrastructure.PurgeQueue("engine.provider.validation");
        
        _messagingInfrastructure.BindQueue("engine.provider.validation", Exchanges.Provider, ProviderRoutingKeys.ValidationRequested);
        
        _subscriber.Subscribe<ValidateProviderCapabilityCommand>("engine.provider.validation", HandleValidationRequest);
    }

    private void HandleValidationRequest(ValidateProviderCapabilityCommand request)
    {
        try
        {
            var validationResult = ValidateProvider(request);

            if (validationResult.IsValid)
            {
                var approvedResponse = new ProviderCapabilityValidationApprovedEvent
                {
                    CommandId = request.CommandId,
                    ProviderId = request.ProviderId,
                    ProviderType = request.ProviderType,
                    ProviderName = request.ProviderName
                };

                _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ValidationApproved, approvedResponse);
            }
            else
            {
                var declinedResponse = new ProviderCapabilityValidationDeclinedEvent
                {
                    CommandId = request.CommandId,
                    ProviderId = request.ProviderId,
                    ProviderType = request.ProviderType,
                    ProviderName = request.ProviderName,
                    Reason = validationResult.Reason
                };

                _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ValidationDeclined, declinedResponse);
            }
        }
        catch (Exception ex)
        {
            var errorResponse = new ProviderCapabilityValidationDeclinedEvent
            {
                CommandId = request.CommandId,
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType,
                ProviderName = request.ProviderName,
                Reason = $"Internal error: {ex.Message}"
            };

            _publisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ValidationDeclined, errorResponse);
        }
    }

    private ValidationResult ValidateProvider(ValidateProviderCapabilityCommand request)
    {
        // Validate capabilities
        var capabilitiesResult = ValidateCapabilities(request.ProviderType, request.Capabilities);
        if (!capabilitiesResult.IsValid)
        {
            return capabilitiesResult;
        }

        // Validate technical requirements
        var technicalResult = ValidateTechnicalRequirements(request.TechnicalCapabilities);
        if (!technicalResult.IsValid)
        {
            return technicalResult;
        }

        return new ValidationResult { IsValid = true };
    }

    private ValidationResult ValidateCapabilities(string providerType, List<string>? capabilities)
    {
        var expectedCapabilities = providerType switch
        {
            "MainRemanufacturingCenter" => _settings.RequiredCapabilities.MainRemanufacturingCenter,
            "EngineeringDesignFirm" => _settings.RequiredCapabilities.EngineeringDesignFirm,
            "PrecisionMachineShop" => _settings.RequiredCapabilities.PrecisionMachineShop,
            _ => null
        };

        if (expectedCapabilities == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Reason = $"Unknown provider type: {providerType}"
            };
        }

        if (capabilities == null || capabilities.Count == 0)
        {
            return new ValidationResult
            {
                IsValid = false,
                Reason = "No capabilities provided"
            };
        }

        var missingCapabilities = expectedCapabilities.Except(capabilities).ToList();
        if (missingCapabilities.Any())
        {
            return new ValidationResult
            {
                IsValid = false,
                Reason = $"Missing required capabilities: {string.Join(", ", missingCapabilities)}"
            };
        }

        var unexpectedCapabilities = capabilities.Except(expectedCapabilities).ToList();
        if (unexpectedCapabilities.Any())
        {
            return new ValidationResult
            {
                IsValid = false,
                Reason = $"Unexpected capabilities: {string.Join(", ", unexpectedCapabilities)}"
            };
        }

        return new ValidationResult { IsValid = true };
    }

    private ValidationResult ValidateTechnicalRequirements(TechnicalCapabilities? requirements)
    {
        if (requirements == null)
        {
            return new ValidationResult { IsValid = true }; // Optional
        }

        var limits = _settings.TechnicalLimits;

        // Validate AxisHeight
        if (requirements.AxisHeight > 0)
        {
            if (requirements.AxisHeight < limits.MinAxisHeight || requirements.AxisHeight > limits.MaxAxisHeight)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Reason = $"AxisHeight {requirements.AxisHeight} out of range [{limits.MinAxisHeight}-{limits.MaxAxisHeight}]"
                };
            }
        }

        // Validate Power
        if (requirements.Power > 0)
        {
            if (requirements.Power < limits.MinPower || requirements.Power > limits.MaxPower)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Reason = $"Power {requirements.Power} out of range [{limits.MinPower}-{limits.MaxPower}]"
                };
            }
        }

        // Validate Tolerance
        if (requirements.Tolerance > 0)
        {
            if (requirements.Tolerance < limits.MinTolerance || requirements.Tolerance > limits.MaxTolerance)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Reason = $"Tolerance {requirements.Tolerance} out of range [{limits.MinTolerance}-{limits.MaxTolerance}]"
                };
            }
        }

        return new ValidationResult { IsValid = true };
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
