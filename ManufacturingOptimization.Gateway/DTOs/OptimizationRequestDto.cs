namespace ManufacturingOptimization.Gateway.DTOs
{
    public class OptimizationRequestDto
    {
        public Guid RequestId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecificationsDto MotorSpecs { get; set; } = new();
        public OptimizationRequestConstraintsDto Constraints { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class MotorSpecificationsDto
    {
        public double PowerKW { get; set; }
        public int AxisHeightMM { get; set; }
        public string CurrentEfficiency { get; set; } = string.Empty;
        public string TargetEfficiency { get; set; } = string.Empty;
        public string? MalfunctionDescription { get; set; }
    }

    public class OptimizationRequestConstraintsDto
    {
        public decimal? MaxBudget { get; set; }
        public DateTime? RequiredDeadline { get; set; }
    }
}
