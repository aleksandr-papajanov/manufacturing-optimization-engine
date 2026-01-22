using ManufacturingOptimization.Common.Models.Enums;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class MotorSpecificationsDto
    {
        public double PowerKW { get; set; }
        public int AxisHeightMM { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MotorEfficiencyClass CurrentEfficiency { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MotorEfficiencyClass TargetEfficiency { get; set; }
        public string? MalfunctionDescription { get; set; }
    }
}