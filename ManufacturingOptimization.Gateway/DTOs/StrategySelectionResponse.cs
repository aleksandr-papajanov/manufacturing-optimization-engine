namespace ManufacturingOptimization.Gateway.DTOs
{
    public class StrategySelectionResponse
    {
        public string Status { get; set; } = string.Empty;
        public Guid RequestId { get; set; }
        public Guid StrategyId { get; set; }
    }
}
