namespace ManufacturingOptimization.Gateway.Controllers
{
    public partial class OptimizationController
    {
        // --- DTOs ---

        public class MotorRequestDto
        {
            public string RequestId { get; set; } = string.Empty;
            public string CustomerId { get; set; } = string.Empty;
            public string Power { get; set; } = string.Empty;
            public string TargetEfficiency { get; set; } = string.Empty;
        }
    }
}