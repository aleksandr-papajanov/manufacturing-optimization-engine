namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Standard error response for API endpoints.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
