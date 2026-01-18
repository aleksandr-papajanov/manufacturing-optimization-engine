using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Models
{
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
        public MotorEfficiencyClass CurrentEfficiency { get; set; }

        /// <summary>
        /// Desired efficiency. IE4 triggers "Upgrade" strategy; IE2 triggers "Refurbish".
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MotorEfficiencyClass TargetEfficiency { get; set; }
        
        public string? MalfunctionDescription { get; set; }
    }
}