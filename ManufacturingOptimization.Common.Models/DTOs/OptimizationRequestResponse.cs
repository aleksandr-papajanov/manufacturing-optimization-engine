namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Response returned when an optimization request is submitted.
/// </summary>
public class OptimizationRequestResponse
{
    /// <summary>
    /// Status message indicating request acceptance.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the command.
    /// </summary>
    public Guid CommandId { get; set; }

    /// <summary>
    /// Unique identifier for the optimization request.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Human-readable message about the request processing.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
