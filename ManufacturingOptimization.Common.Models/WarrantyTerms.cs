namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Warranty terms associated with an optimization strategy.
/// </summary>
public class WarrantyTerms
{
    /// <summary>
    /// Unique identifier for warranty terms.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Warranty level (e.g., "Basic", "Silver", "Gold", "Platinum")
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Duration in months
    /// </summary>
    public int DurationMonths { get; set; }

    /// <summary>
    /// Human-readable description (e.g., "Platinum 3 Years")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether insurance coverage is included
    /// </summary>
    public bool IncludesInsurance { get; set; }
}
