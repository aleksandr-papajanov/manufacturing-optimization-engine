using System;
using System.Text.Json.Serialization;

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
        public RequestConstraints Constraints { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class MotorSpecifications
    {
        /// <summary>
        /// Power in kW. (Constraint: TP1 handles ~5.5kW)
        /// </summary>
        public double PowerKW { get; set; }

        /// <summary>
        /// Axis height in mm. (Constraint: TP1 handles ~75mm)
        /// </summary>
        public int AxisHeightMM { get; set; }

        /// <summary>
        /// Current efficiency of the motor (e.g., IE1, IE2).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EfficiencyClass CurrentEfficiency { get; set; }

        /// <summary>
        /// Desired efficiency. IE4 triggers "Upgrade" strategy; IE2 triggers "Refurbish".
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public EfficiencyClass TargetEfficiency { get; set; }
        
        public string? MalfunctionDescription { get; set; }
    }

    public class RequestConstraints
    {
        public decimal MaxBudget { get; set; }
        
        public DateTime? RequiredDeadline { get; set; }

        /// <summary>
        /// Prioritize: Cost, Time, or Quality.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OptimizationPriority Priority { get; set; }
    }

    public enum EfficiencyClass
    {
        Unknown = 0,
        IE1, // Standard
        IE2, // High (Refurbish Target)
        IE3, // Premium
        IE4  // Super Premium (Upgrade Target)
    }

    public enum OptimizationPriority
    {
        LowestCost,
        FastestDelivery,
        HighestQuality 
    }
}