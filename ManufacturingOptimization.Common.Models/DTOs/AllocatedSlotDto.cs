namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class AllocatedSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<TimeSegmentDto> Segments { get; set; } = new();
    }
}
