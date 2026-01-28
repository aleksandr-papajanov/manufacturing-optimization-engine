using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class OptimizationRequestDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecificationsDto MotorSpecs { get; set; } = new();
        public OptimizationRequestConstraintsDto Constraints { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}