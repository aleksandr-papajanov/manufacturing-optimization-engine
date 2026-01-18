namespace ManufacturingOptimization.Gateway.Controllers
{
    public partial class OptimizationController
    {
        // NEW: DTO for Strategy Selection (US-07-T4)
        public class SelectStrategyDto
        {
            public Guid RequestId { get; set; }
            public Guid StrategyId { get; set; }
            public string StrategyName { get; set; } = string.Empty;
        }
    }
}