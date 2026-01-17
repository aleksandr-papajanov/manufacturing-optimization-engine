using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Settings;
using Microsoft.Extensions.Options;
using Common.Models;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// Handles provider validation requests and tracks provider registration.
/// Publishes AllProvidersReadyEvent when all approved providers are registered.
/// </summary>
public class ProviderCapabilityValidationService : BackgroundService
{
    private readonly ILogger<ProviderCapabilityValidationService> _logger;
    private readonly IMessageSubscriber _subscriber;
    private readonly IMessagePublisher _publisher;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly ProviderValidationSettings _settings;
    
    private readonly ConcurrentDictionary<Guid, string> _approvedProviders = new();
    private readonly ConcurrentDictionary<Guid, string> _registeredProviders = new();

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
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareQueue("engine.provider.validation");
        _messagingInfrastructure.BindQueue("engine.provider.validation", Exchanges.Provider, ProviderRoutingKeys.ValidationRequested);
        _messagingInfrastructure.PurgeQueue("engine.provider.validation");
        
        _subscriber.Subscribe<ValidateProviderCapabilityCommand>("engine.provider.validation", HandleValidationRequest);
    }

    private void HandleValidationRequest(ValidateProviderCapabilityCommand request)
    {
        ProviderCapabilityValidatedEvent response;
        
        try
        {
            var validationResult = ValidateProvider(request);

            response = new ProviderCapabilityValidatedEvent
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType,
                ProviderName = request.ProviderName,
                IsApproved = validationResult.IsValid,
                Reason = validationResult.IsValid ? null : validationResult.Reason,
                CommandId = request.CommandId
            };
        }
        catch (Exception ex)
        {
            response = new ProviderCapabilityValidatedEvent
            {
                ProviderId = request.ProviderId,
                ProviderType = request.ProviderType,
                ProviderName = request.ProviderName,
                IsApproved = false,
                Reason = $"Internal error: {ex.Message}",
                CommandId = request.CommandId
            };
        }

        _publisher.PublishReply(request.ReplyTo, request.CommandId.ToString(), response);
    }

    private ValidationResult ValidateProvider(ValidateProviderCapabilityCommand request)
    {
        // Validate process capabilities
        var processNames = request.ProcessCapabilities.Select(pc => pc.ProcessName).ToList();
        var capabilitiesResult = ValidateCapabilities(request.ProviderType, processNames);
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
