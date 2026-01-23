using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.ProviderRegistry.Abstractions;

/// <summary>
/// Service for validating provider capabilities via RPC.
/// </summary>
public interface IProviderValidationService
{
    /// <summary>
    /// Validates provider capabilities by sending RPC request to Engine.
    /// </summary>
    /// <param name="provider">Provider to validate</param>
    /// <param name="timeout">Validation timeout</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if approved, false if rejected or timeout</returns>
    Task<(bool IsApproved, string? DeclinedReason)> ValidateAsync(ProviderModel provider, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
}
