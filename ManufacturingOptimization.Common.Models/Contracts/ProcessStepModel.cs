using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Contracts;

/// <summary>
/// Single step in optimized workflow with selected provider.
/// </summary>
public class ProcessStepModel
{
    /// <summary>
    /// Unique identifier for this step.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public int StepNumber { get; set; }
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// Selected provider for this step.
    /// </summary>
    public Guid SelectedProviderId { get; set; }
    
    /// <summary>
    /// Selected provider name for display.
    /// </summary>
    public string SelectedProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// Process estimate from selected provider.
    /// </summary>
    public ProcessEstimateModel Estimate { get; set; } = new();
    
    /// <summary>
    /// The allocated time slot for this step with detailed segment breakdown (working time and breaks).
    /// </summary>
    public AllocatedSlotModel? AllocatedSlot { get; set; }
}
