using Google.OrTools.LinearSolver;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Generates multiple optimization strategies using Google OR-Tools.
/// Each strategy optimizes for a different priority (Cost, Time, Quality, Emissions).
/// </summary>
public partial class OptimizationStep : IWorkflowStep
{
    private static readonly OptimizationPriority[] PRIORITIES_TO_GENERATE = new[]
    {
        OptimizationPriority.LowestCost,
        OptimizationPriority.FastestDelivery,
        OptimizationPriority.HighestQuality,
        OptimizationPriority.LowestEmissions
    };
    private readonly ILogger<OptimizationStep> _logger;
    
    
    public OptimizationStep(ILogger<OptimizationStep> logger)
    {
        _logger = logger;
    }

    public string Name => "Optimization & Strategy Generation";

    /// <summary>
    /// Execute optimization - generates multiple strategies.
    /// </summary>
    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating optimization strategies...");

        // Validate that all steps have providers
        if (context.ProcessSteps.Any(s => s.MatchedProviders.Count == 0))
        {
            context.Errors.Add("Cannot optimize: some steps have no matched providers");
            return;
        }

        // Generate strategies for each priority
        foreach (var priority in PRIORITIES_TO_GENERATE)
        {
            try
            {
                // Create a snapshot of process steps for this optimization
                var stepsSnapshot = CreateProcessStepsSnapshot(context.ProcessSteps);
                
                // Create temporary context for this optimization
                var tempContext = new WorkflowContext
                {
                    Request = context.Request,
                    WorkflowType = context.WorkflowType,
                    ProcessSteps = stepsSnapshot
                };

                // Run optimization with this priority
                var result = await OptimizeForPriorityAsync(tempContext, priority, cancellationToken);

                if (result != null)
                {
                    // Create strategy from optimization result
                    var strategy = CreateStrategy(priority, tempContext, result);
                    context.Strategies.Add(strategy);

                    _logger.LogInformation(
                        "Generated {StrategyName}: Cost=${Cost}, Time={Hours}h, Quality={Quality:F2}, CO2={Emissions}kg",
                        strategy.StrategyName, result.TotalCost, result.TotalDuration.TotalHours,
                        result.AverageQuality, result.TotalEmissionsKgCO2);
                }
                else
                {
                    _logger.LogWarning("Failed to generate strategy for priority: {Priority}", priority);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating strategy for priority: {Priority}", priority);
            }
        }

        if (context.Strategies.Count == 0)
        {
            context.Errors.Add("Failed to generate any optimization strategies");
            _logger.LogError("No strategies generated");
        }
        else
        {
            _logger.LogInformation("Successfully generated {Count} strategies", context.Strategies.Count);
            
            // Apply constraint filtering
            var filteredStrategies = ApplyConstraintFiltering(context);
            
            if (filteredStrategies.Count == 0)
            {
                _logger.LogWarning("All strategies filtered out by constraints. Showing all strategies with warnings.");
                // Keep all strategies but user should be aware they don't meet constraints
            }
            else if (filteredStrategies.Count < context.Strategies.Count)
            {
                _logger.LogInformation("Filtered {RemovedCount} strategies that don't meet constraints. {RemainingCount} strategies remain.",
                    context.Strategies.Count - filteredStrategies.Count, filteredStrategies.Count);
                context.Strategies = filteredStrategies;
            }
        }
    }

    /// <summary>
    /// Filter strategies based on MaxBudget and RequiredDeadline constraints.
    /// </summary>
    private List<OptimizationStrategy> ApplyConstraintFiltering(WorkflowContext context)
    {
        var constraints = context.Request.Constraints;
        var filteredStrategies = new List<OptimizationStrategy>(context.Strategies);

        // Filter by MaxBudget if specified
        if (constraints.MaxBudget.HasValue)
        {
            var beforeCount = filteredStrategies.Count;
            filteredStrategies = filteredStrategies
                .Where(s => s.Metrics.TotalCost <= constraints.MaxBudget.Value)
                .ToList();
            
            if (filteredStrategies.Count < beforeCount)
            {
                _logger.LogInformation("Filtered {Count} strategies exceeding MaxBudget of €{Budget}",
                    beforeCount - filteredStrategies.Count, constraints.MaxBudget.Value);
            }
        }

        // Filter by RequiredDeadline if specified
        if (constraints.RequiredDeadline.HasValue)
        {
            var beforeCount = filteredStrategies.Count;
            var maxAllowedHours = (constraints.RequiredDeadline.Value - DateTime.Now).TotalHours;
            
            filteredStrategies = filteredStrategies
                .Where(s => s.Metrics.TotalDuration.TotalHours <= maxAllowedHours)
                .ToList();
            
            if (filteredStrategies.Count < beforeCount)
            {
                _logger.LogInformation("Filtered {Count} strategies that cannot meet deadline of {Deadline}",
                    beforeCount - filteredStrategies.Count, constraints.RequiredDeadline.Value.ToString("yyyy-MM-dd"));
            }
        }

        return filteredStrategies;
    }

    
    /// <summary>
    /// Execute optimization with specific priority.
    /// </summary>
    private Task<OptimizationMetrics?> OptimizeForPriorityAsync(
        WorkflowContext context, 
        OptimizationPriority priority,
        CancellationToken cancellationToken = default)
    {
        // Get weights based on priority
        var weights = GetWeightsForPriority(priority);
        
        // Create solver
        Solver solver = Solver.CreateSolver("SCIP");
        if (solver == null)
        {
            return Task.FromResult<OptimizationMetrics?>(null);
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

            // OBJECTIVE: Minimize weighted sum based on priority
            var objective = solver.Objective();
            
            for (int i = 0; i < context.ProcessSteps.Count; i++)
            {
                var step = context.ProcessSteps[i];
                
                for (int j = 0; j < step.MatchedProviders.Count; j++)
                {
                    var provider = step.MatchedProviders[j];
                    var key = (i, j);
                    
                    // Normalize values to 0-1 range
                    double normalizedCost = (double)provider.CostEstimate / 2000.0;
                    double normalizedTime = provider.TimeEstimate.TotalHours / 40.0;
                    double normalizedQuality = provider.QualityScore; // already 0-1
                    double normalizedEmissions = provider.EmissionsKgCO2 / 100.0; // normalize
                    
                    // Apply weights based on priority
                    double coefficient = 
                        weights.CostWeight * normalizedCost +
                        weights.TimeWeight * normalizedTime +
                        weights.EmissionsWeight * normalizedEmissions -
                        weights.QualityWeight * normalizedQuality; // negative = maximize
                    
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
                double totalEmissions = 0;
                
                for (int i = 0; i < context.ProcessSteps.Count; i++)
                {
                    var step = context.ProcessSteps[i];
                    
                    for (int j = 0; j < step.MatchedProviders.Count; j++)
                    {
                        var key = (i, j);
                        
                        // Check if this provider was selected
                        if (assignments[key].SolutionValue() > 0.5)
                        {
                            var provider = step.MatchedProviders[j];
                            step.SelectedProvider = provider;
                            
                            totalCost += provider.CostEstimate;
                            totalTime += provider.TimeEstimate;
                            totalQuality += provider.QualityScore;
                            totalEmissions += provider.EmissionsKgCO2;
                            
                            break;
                        }
                    }
                }
                
                var result = new OptimizationMetrics
                {
                    TotalCost = totalCost,
                    TotalDuration = totalTime,
                    AverageQuality = totalQuality / context.ProcessSteps.Count,
                    TotalEmissionsKgCO2 = totalEmissions,
                    SolverStatus = status.ToString(),
                    ObjectiveValue = objective.Value()
                };
                
                return Task.FromResult<OptimizationMetrics?>(result);
            }
            else
            {
                _logger.LogError("Optimization failed: {Status}", status);
                return Task.FromResult<OptimizationMetrics?>(null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Optimization error");
            return Task.FromResult<OptimizationMetrics?>(null);
        }
    }
    
    /// <summary>
    /// Create a deep copy of process steps.
    /// </summary>
    private static List<WorkflowProcessStep> CreateProcessStepsSnapshot(List<WorkflowProcessStep> originalSteps)
    {
        return originalSteps.Select(step => new WorkflowProcessStep
        {
            StepNumber = step.StepNumber,
            Activity = step.Activity,
            RequiredCapability = step.RequiredCapability,
            Description = step.Description,
            MatchedProviders = step.MatchedProviders.Select(p => new MatchedProvider
            {
                ProviderId = p.ProviderId,
                ProviderName = p.ProviderName,
                ProviderType = p.ProviderType,
                CostEstimate = p.CostEstimate,
                TimeEstimate = p.TimeEstimate,
                QualityScore = p.QualityScore,
                EmissionsKgCO2 = p.EmissionsKgCO2
            }).ToList()
        }).ToList();
    }
    
    /// <summary>
    /// Create an optimization strategy from optimization result.
    /// </summary>
    private OptimizationStrategy CreateStrategy(
        OptimizationPriority priority,
        WorkflowContext context,
        OptimizationMetrics result)
    {
        var (strategyName, description) = GetStrategyNameAndDescription(priority);
        var (warranty, insurance) = GetWarrantyAndInsurance(priority, context.WorkflowType ?? "Refurbish");

        return new OptimizationStrategy
        {
            StrategyName = strategyName,
            Priority = priority,
            WorkflowType = context.WorkflowType ?? "Unknown",
            Steps = context.ProcessSteps
                .Where(s => s.SelectedProvider != null)
                .Select(s => new OptimizedProcessStep
                {
                    StepNumber = s.StepNumber,
                    Activity = s.Activity,
                    SelectedProviderId = s.SelectedProvider!.ProviderId,
                    SelectedProviderName = s.SelectedProvider!.ProviderName,
                    CostEstimate = s.SelectedProvider!.CostEstimate,
                    TimeEstimate = s.SelectedProvider!.TimeEstimate,
                    QualityScore = s.SelectedProvider!.QualityScore,
                    EmissionsKgCO2 = s.SelectedProvider!.EmissionsKgCO2
                }).ToList(),
            Metrics = new OptimizationMetrics
            {
                TotalCost = result.TotalCost,
                TotalDuration = result.TotalDuration,
                AverageQuality = result.AverageQuality,
                TotalEmissionsKgCO2 = result.TotalEmissionsKgCO2,
                SolverStatus = result.SolverStatus,
                ObjectiveValue = result.ObjectiveValue
            },
            WarrantyTerms = warranty,
            IncludesInsurance = insurance,
            Description = description
        };
    }
    
    /// <summary>
    /// Get strategy name and description based on priority.
    /// </summary>
    private static (string name, string description) GetStrategyNameAndDescription(OptimizationPriority priority)
    {
        return priority switch
        {
            OptimizationPriority.LowestCost => ("Budget Strategy", "Optimized for lowest total cost. Best for price-sensitive customers."),
            OptimizationPriority.FastestDelivery => ("Express Strategy", "Optimized for fastest completion time. Best for urgent orders."),
            OptimizationPriority.HighestQuality => ("Premium Strategy", "Optimized for highest quality and reliability. Best long-term value."),
            OptimizationPriority.LowestEmissions => ("Eco Strategy", "Optimized for minimal carbon emissions. Best for sustainability goals."),
            _ => ("Balanced Strategy", "Balanced optimization across all factors.")
        };
    }

    /// <summary>
    /// Determine warranty terms and insurance based on priority and workflow type.
    /// </summary>
    private (string warranty, bool insurance) GetWarrantyAndInsurance(OptimizationPriority priority, string workflowType)
    {
        bool isUpgrade = workflowType == "Upgrade";

        return priority switch
        {
            OptimizationPriority.HighestQuality => isUpgrade
                ? ("Platinum 3 Years", true)
                : ("Gold 18 Months", true),

            OptimizationPriority.FastestDelivery => isUpgrade
                ? ("Gold 12 Months", true)
                : ("Silver 6 Months", false),

            OptimizationPriority.LowestEmissions => isUpgrade
                ? ("Gold 12 Months", true)
                : ("Silver 9 Months", true),

            OptimizationPriority.LowestCost => 
                ("Basic 3 Months", false),

            _ => ("Standard 6 Months", false)
        };
    }
    
    /// <summary>
    /// Get optimization weights based on priority.
    /// </summary>
    private static OptimizationWeights GetWeightsForPriority(OptimizationPriority priority)
    {
        return priority switch
        {
            OptimizationPriority.LowestCost => new OptimizationWeights
            {
                CostWeight = 0.8,      // Heavily prioritize cost
                TimeWeight = 0.1,
                QualityWeight = 0.05,
                EmissionsWeight = 0.05
            },
            OptimizationPriority.FastestDelivery => new OptimizationWeights
            {
                CostWeight = 0.1,
                TimeWeight = 0.8,      // Heavily prioritize time
                QualityWeight = 0.05,
                EmissionsWeight = 0.05
            },
            OptimizationPriority.HighestQuality => new OptimizationWeights
            {
                CostWeight = 0.2,
                TimeWeight = 0.2,
                QualityWeight = 0.5,   // Heavily prioritize quality
                EmissionsWeight = 0.1
            },
            OptimizationPriority.LowestEmissions => new OptimizationWeights
            {
                CostWeight = 0.1,
                TimeWeight = 0.1,
                QualityWeight = 0.2,
                EmissionsWeight = 0.6  // Heavily prioritize low emissions
            },
            _ => new OptimizationWeights // Balanced default
            {
                CostWeight = 0.3,
                TimeWeight = 0.3,
                QualityWeight = 0.3,
                EmissionsWeight = 0.1
            }
        };
    }
}
