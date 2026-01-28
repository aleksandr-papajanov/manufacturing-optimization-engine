namespace ManufacturingOptimization.Engine.Models;

public class ProviderRecommendation
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    
    // Existing metrics
    public double MatchScore { get; set; }
    public decimal EstimatedCost { get; set; }
    public double EstimatedLeadTimeDays { get; set; }
    public double SustainabilityRating { get; set; }

    // NEW FIELDS [US-07-T5]
    public string WarrantyTerms { get; set; } = string.Empty; // e.g., "12 Months Parts & Labor"
    public bool IncludesInsurance { get; set; } = false;      // e.g., true/false
}