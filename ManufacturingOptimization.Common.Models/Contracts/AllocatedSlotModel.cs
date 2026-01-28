namespace ManufacturingOptimization.Common.Models.Contracts
{
    public class AllocatedSlotModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public List<TimeSegmentModel> Segments { get; set; } = new();
    }
}
