namespace ManufacturingOptimization.Gateway.DTOs
{
    public class OptimizationStrategyDto
    {
        public Guid StrategyId { get; set; }
        public string StrategyName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string WorkflowType { get; set; } = string.Empty;
        public List<OptimizationProcessStepDto> Steps { get; set; } = new();
        public OptimizationMetricsDto Metrics { get; set; } = new();
        public string WarrantyTerms { get; set; } = string.Empty;
        public bool IncludesInsurance { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class OptimizationProcessStepDto
    {
        public int StepNumber { get; set; }
        public string Activity { get; set; } = string.Empty;
        public Guid SelectedProviderId { get; set; }
        public string SelectedProviderName { get; set; } = string.Empty;
        public ProcessEstimateDto Estimate { get; set; } = new();
    }

    public class ProcessEstimateDto
    {
        public decimal Cost { get; set; }
        public TimeSpan Duration { get; set; }
        public double QualityScore { get; set; }
        public double EmissionsKgCO2 { get; set; }
    }

    public class OptimizationMetricsDto
    {
        public decimal TotalCost { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double AverageQuality { get; set; }
        public double TotalEmissionsKgCO2 { get; set; }
        public string SolverStatus { get; set; } = string.Empty;
        public double ObjectiveValue { get; set; }
    }
}
