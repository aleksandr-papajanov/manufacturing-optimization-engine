using Common.Models;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Step 1: Workflow Matching
/// Determines workflow type (Upgrade vs Refurbish) and generates process steps.
/// </summary>
public class WorkflowMatchingStep : IWorkflowStep
{
    private readonly ILogger<WorkflowMatchingStep> _logger;

    public string Name => "Workflow Matching";

    public WorkflowMatchingStep(ILogger<WorkflowMatchingStep> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        var targetEfficiency = context.Request.Specs.TargetEfficiency;
        
        // Determine workflow type
        bool isUpgrade = targetEfficiency == MotorEfficiencyClass.IE4;
        context.WorkflowType = isUpgrade ? "Upgrade" : "Refurbish";
        
        // Generate process steps
        context.ProcessSteps = isUpgrade 
            ? CreateUpgradeSteps() 
            : CreateRefurbishSteps();
        
        _logger.LogInformation("Workflow: {WorkflowType} ({StepCount} steps)", 
            context.WorkflowType, context.ProcessSteps.Count);
        
        return Task.CompletedTask;
    }

    private static List<WorkflowProcessStep> CreateUpgradeSteps()
    {
        return new List<WorkflowProcessStep>
        {
            new() { StepNumber = 1, Activity = "Cleaning", RequiredCapability = "Cleaning", Description = "Remove dirt and contaminants" },
            new() { StepNumber = 2, Activity = "Disassembly", RequiredCapability = "Disassembly", Description = "Take motor completely apart" },
            new() { StepNumber = 3, Activity = "Redesign", RequiredCapability = "Redesign", Description = "Engineer improved components" },
            new() { StepNumber = 4, Activity = "Turning", RequiredCapability = "Turning", Description = "Machine parts on lathe (150mm x 70mm)" },
            new() { StepNumber = 5, Activity = "Grinding", RequiredCapability = "Grinding", Description = "Precision surface finishing" },
            new() { StepNumber = 6, Activity = "Part Replacement", RequiredCapability = "PartSubstitution", Description = "Install new/upgraded parts" },
            new() { StepNumber = 7, Activity = "Reassembly", RequiredCapability = "Reassembly", Description = "Put motor back together" },
            new() { StepNumber = 8, Activity = "Certification", RequiredCapability = "Certification", Description = "Test for IE4 compliance" }
        };
    }

    private static List<WorkflowProcessStep> CreateRefurbishSteps()
    {
        return new List<WorkflowProcessStep>
        {
            new() { StepNumber = 1, Activity = "Cleaning", RequiredCapability = "Cleaning", Description = "Remove dirt and contaminants" },
            new() { StepNumber = 2, Activity = "Disassembly", RequiredCapability = "Disassembly", Description = "Take motor apart" },
            new() { StepNumber = 3, Activity = "Part Replacement", RequiredCapability = "PartSubstitution", Description = "Replace worn parts" },
            new() { StepNumber = 4, Activity = "Reassembly", RequiredCapability = "Reassembly", Description = "Put motor back together" },
            new() { StepNumber = 5, Activity = "Certification", RequiredCapability = "Certification", Description = "Test for IE2 compliance" }
        };
    }
}
