using System;

namespace Common.Models
{
    /// <summary>
    /// Represents a customer request for motor remanufacturing.
    /// Maps to User Story US-06.
    /// </summary>
    public class MotorRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = string.Empty;

        // Core motor attributes required for TP validation
        public MotorSpecifications Specs { get; set; } = new();

        // Constraints for the optimization engine (Budget, Time)
        public MotorRequestConstraints Constraints { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}