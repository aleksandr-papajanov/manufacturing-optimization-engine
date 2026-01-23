namespace ManufacturingOptimization.Common.Models.Contracts
{
    public class OptimizationRequestConstraintsModel
    {
        /// <summary>
        /// Maximum acceptable cost for the optimization plan.
        /// If specified, strategies exceeding this budget will be filtered out.
        /// </summary>
        public decimal? MaxBudget { get; set; }
        
        /// <summary>
        /// Required completion deadline for the optimization plan.
        /// If specified, strategies that cannot meet this deadline will be filtered out.
        /// </summary>
        public DateTime? RequiredDeadline { get; set; }
    }
}