namespace ManufacturingOptimization.Common.Models
{
    /// <summary>
    /// Represents a customer request for motor remanufacturing.
    /// Maps to User Story US-06.
    /// </summary>
    public class OptimizationRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecifications MotorSpecs { get; set; } = new();
        public OptimizationRequestConstraints Constraints { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}