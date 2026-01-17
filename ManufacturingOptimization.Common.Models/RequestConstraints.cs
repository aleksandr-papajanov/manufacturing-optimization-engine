using System.Text.Json.Serialization;

namespace Common.Models
{
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
}