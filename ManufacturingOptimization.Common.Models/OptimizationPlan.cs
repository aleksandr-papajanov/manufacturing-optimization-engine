namespace Common.Models;

/// <summary>
/// Optimized plan with provider assignments for motor remanufacturing workflow.
/// Used for communication between Engine and other services (Gateway, Analytics).
/// </summary>
public class OptimizationPlan
{
    public Guid PlanId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish".
    /// </summary>
    public string WorkflowType { get; set; } = string.Empty;
    
    /// <summary>
    /// Sequential process steps with selected providers.
    /// </summary>
    public List<OptimizedProcessStep> Steps { get; set; } = new();
    
    /// <summary>
    /// Total estimated cost for the entire workflow.
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Total estimated duration for the entire workflow.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
    
    /// <summary>
    /// Average quality score across all steps (0.0 - 1.0).
    /// </summary>
    public double AverageQuality { get; set; }
    
    /// <summary>
    /// Total estimated carbon emissions in kg CO2.
    /// </summary>
    public double TotalEmissionsKgCO2 { get; set; }
    
    /// <summary>
    /// Optimization solver status (OPTIMAL, FEASIBLE, etc.).
    /// </summary>
    public string SolverStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the plan was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indicates whether the optimization was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;
    
    /// <summary>
    /// List of errors encountered during pipeline execution.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
