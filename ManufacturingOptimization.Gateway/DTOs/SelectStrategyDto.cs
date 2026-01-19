namespace ManufacturingOptimization.Gateway.DTOs
{
    public class SelectStrategyDto
    {
        public Guid RequestId { get; set; }
        public Guid StrategyId { get; set; }
        public string StrategyName { get; set; } = string.Empty;
    }
}