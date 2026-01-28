namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class OptimizationRequestConstraintsDto
    {
        public decimal? MaxBudget { get; set; }
        public TimeWindowDto TimeWindow { get; set; } = null!;
    }
}