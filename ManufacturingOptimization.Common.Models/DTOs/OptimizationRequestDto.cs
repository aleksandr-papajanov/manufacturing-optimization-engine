using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class OptimizationRequestDto
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecificationsModel MotorSpecs { get; set; } = new();
        public OptimizationRequestConstraintsModel Constraints { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}