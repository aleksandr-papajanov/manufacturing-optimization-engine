using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Command to validate provider capabilities before registration/startup.
/// Published by ProviderRegistry, consumed by Engine.
/// </summary>
public class ValidateProviderCapabilityCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public TechnicalRequirementsDto TechnicalRequirements { get; set; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}

public class TechnicalRequirementsDto
{
    public double AxisHeight { get; set; }
    public double Power { get; set; }
    public double Tolerance { get; set; }
}
