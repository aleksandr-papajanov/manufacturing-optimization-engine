using Google.OrTools.LinearSolver;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Uses Google OR-Tools to solve multi-objective optimization:
/// - Minimize total cost
/// - Minimize total time
/// - Maximize average quality
/// </summary>
public class OptimizationStep : IPipelineStep
{
    private readonly ILogger<OptimizationStep> _logger;
    
    // Weights for multi-objective function
    private const double COST_WEIGHT = 0.5;
    private const double TIME_WEIGHT = 0.3;
    private const double QUALITY_WEIGHT = 0.2;

    public OptimizationStep(ILogger<OptimizationStep> logger)
    {
        _logger = logger;
    }

    public string Name => "Optimization";

    public Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Validate that all steps have providers
        if (context.ProcessSteps.Any(s => s.MatchedProviders.Count == 0))
        {
            context.Errors.Add("Cannot optimize: some steps have no matched providers");
            return Task.CompletedTask;
        }

        // Create solver
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
        {
            context.Errors.Add("Optimization solver not available");
            return Task.CompletedTask;
        }

        try
        {
            // Decision variables: x[i,j] = 1 if provider j is assigned to step i
            var assignments = new Dictionary<(int stepIdx, int providerIdx), Variable>();
            
            for (int i = 0; i < context.ProcessSteps.Count; i++)
            {
                var step = context.ProcessSteps[i];
                for (int j = 0; j < step.MatchedProviders.Count; j++)
                {
                    var key = (i, j);
                    assignments[key] = solver.MakeBoolVar($"x_{i}_{j}");
                }
            }

            // CONSTRAINT: Each step must have exactly one provider assigned
            for (int i = 0; i < context.ProcessSteps.Count; i++)
            {
                var constraint = solver.MakeConstraint(1, 1, $"one_provider_step_{i}");
                var step = context.ProcessSteps[i];
                
                for (int j = 0; j < step.MatchedProviders.Count; j++)
                {
                    constraint.SetCoefficient(assignments[(i, j)], 1);
                }
            }

            // OBJECTIVE: Minimize weighted sum of cost, time, and negative quality
            var objective = solver.Objective();
            
            for (int i = 0; i < context.ProcessSteps.Count; i++)
            {
                var step = context.ProcessSteps[i];
                
                for (int j = 0; j < step.MatchedProviders.Count; j++)
                {
                    var provider = step.MatchedProviders[j];
                    var key = (i, j);
                    
                    // Multi-objective: cost + time - quality (all normalized)
                    double normalizedCost = (double)provider.CostEstimate / 2000.0; // normalize to ~0-1
                    double normalizedTime = provider.TimeEstimate.TotalHours / 40.0; // normalize to ~0-1
                    double normalizedQuality = provider.QualityScore; // already 0-1
                    
                    double coefficient = 
                        COST_WEIGHT * normalizedCost +
                        TIME_WEIGHT * normalizedTime -
                        QUALITY_WEIGHT * normalizedQuality; // negative because we maximize quality
                    
                    objective.SetCoefficient(assignments[key], coefficient);
                }
            }
            
            objective.SetMinimization();

            // SOLVE
            var status = solver.Solve();

            if (status == Solver.ResultStatus.OPTIMAL || status == Solver.ResultStatus.FEASIBLE)
            {
                // Extract solution
                decimal totalCost = 0;
                TimeSpan totalTime = TimeSpan.Zero;
                double totalQuality = 0;
                
                for (int i = 0; i < context.ProcessSteps.Count; i++)
                {
                    var step = context.ProcessSteps[i];
                    
                    for (int j = 0; j < step.MatchedProviders.Count; j++)
                    {
                        var key = (i, j);
                        
                        // Check if this provider was selected (value close to 1)
                        if (assignments[key].SolutionValue() > 0.5)
                        {
                            var provider = step.MatchedProviders[j];
                            step.SelectedProvider = provider;
                            
                            totalCost += provider.CostEstimate;
                            totalTime += provider.TimeEstimate;
                            totalQuality += provider.QualityScore;
                            
                            _logger.LogInformation("Step {StepNum} ({Activity}): {Provider} | ${Cost} | {Hours}h | Q={Quality:F2}",
                                step.StepNumber, step.Activity, provider.ProviderName,
                                provider.CostEstimate, provider.TimeEstimate.TotalHours, provider.QualityScore);
                            
                            break;
                        }
                    }
                }
                
                context.OptimizationResult = new OptimizationResult
                {
                    TotalCost = totalCost,
                    TotalDuration = totalTime,
                    AverageQuality = totalQuality / context.ProcessSteps.Count,
                    SolverStatus = status.ToString(),
                    ObjectiveValue = objective.Value()
                };
            }
            else
            {
                context.Errors.Add($"Optimization failed: {status}");
            }
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Optimization error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}
