namespace ManufacturingOptimization.Gateway.DTOs
{
    public class StrategiesResponse
    {
        public bool IsReady { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OptimizationStrategyDto>? Strategies { get; set; }
    }
}
