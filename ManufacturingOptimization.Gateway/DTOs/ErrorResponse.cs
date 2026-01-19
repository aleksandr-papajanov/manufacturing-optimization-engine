namespace ManufacturingOptimization.Gateway.DTOs
{
    public class ErrorResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
