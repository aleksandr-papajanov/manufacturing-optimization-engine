namespace ManufacturingOptimization.Gateway.DTOs
{
    public class OptimizationRequestResponse
    {
        public string Status { get; set; } = string.Empty;
        public Guid CommandId { get; set; }
        public Guid RequestId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
