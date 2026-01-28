using Google.OrTools.LinearSolver;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Workflow step responsible for generating multiple optimization strategies
/// using Google OR-Tools.
/// </summary>
public sealed class OptimizationStep : IWorkflowStep
{
    /// All optimization priorities we want to generate strategies for.
    private static readonly OptimizationPriority[] Priorities =
    {
        OptimizationPriority.LowestCost,
        OptimizationPriority.FastestDelivery,
        OptimizationPriority.HighestQuality,
        OptimizationPriority.LowestEmissions
    };

    public string Name => "Optimization & Strategy Generation";

    /// <summary>
    /// Main workflow execution method.
    /// </summary>
    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Ensure all process steps have at least one provider.
        // Optimization is impossible otherwise.
        ValidateProviders(context);

        foreach (var priority in Priorities)
        {
            // Run pure optimization calculation
            var result = await OptimizeForPriorityAsync(context,priority, cancellationToken);

            if (result == null)
                continue;

            // Convert calculation result into a domain strategy
            context.Strategies.Add(CreateStrategy(priority, context, result));
        }

        if (context.Strategies.Count == 0)
        {
            throw new InvalidOperationException("Failed to generate any optimization strategies.");
        }
    }

    /// <summary>
    /// Ensures that every workflow step has at least one matched provider.
    /// </summary>
    private static void ValidateProviders(WorkflowContext context)
    {
        if (context.ProcessSteps.Any(s => s.MatchedProviders.Count == 0))
        {
            throw new InvalidOperationException("Cannot optimize: some steps have no matched providers.");
        }
    }

    /// <summary>
    /// Runs optimization for a specific priority.
    /// </summary>
    private Task<OptimizationResult?> OptimizeForPriorityAsync(WorkflowContext context, OptimizationPriority priority, CancellationToken cancellationToken)
    {
        var weights = priority.GetWeights();

        // Create OR-Tools solver
        var solver = Solver.CreateSolver("SCIP");

        if (solver == null)
            return Task.FromResult<OptimizationResult?>(null);

        try
        {
            // Decision variables:
            // x[i,j] = 1 if provider j is selected for step i
            var assignments = CreateDecisionVariables(solver, context);

            // Constraint:
            // Each process step must select exactly ONE provider
            AddOneProviderPerStepConstraints(solver, context, assignments);

            // Build total cost and time expressions, then add budget and deadline constraints
            var (totalCostExpr, totalTimeExpr) = BuildTotalCostAndTimeExpressions(context, assignments);
            AddBudgetAndDeadlineConstraints(solver, context, totalCostExpr, totalTimeExpr);

            // Objective:
            // Minimize weighted sum based on selected priority
            var objective = BuildObjective(solver, context, assignments, weights);

            objective.SetMinimization();

            var status = solver.Solve();

            if (status is not Solver.ResultStatus.OPTIMAL and not Solver.ResultStatus.FEASIBLE)
            {
                return Task.FromResult<OptimizationResult?>(null);
            }

            return Task.FromResult<OptimizationResult?>(ExtractResult(context, assignments, objective, status));
        }
        catch
        {
            // Fail-safe: optimization errors should not crash workflow
            return Task.FromResult<OptimizationResult?>(null);
        }
    }

    /// <summary>
    /// Creates boolean decision variables for each (step, provider) pair.
    /// </summary>
    private static Dictionary<(int step, int provider), Variable> CreateDecisionVariables(Solver solver,WorkflowContext context)
    {
        var variables = new Dictionary<(int, int), Variable>();

        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];

            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                variables[(i, j)] = solver.MakeBoolVar($"x_{i}_{j}");
            }
        }

        return variables;
    }

    /// <summary>
    /// Adds constraint that each process step must select exactly one provider.
    /// </summary>
    private static void AddOneProviderPerStepConstraints(Solver solver, WorkflowContext context, Dictionary<(int step, int provider), Variable> assignments)
    {
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var constraint = solver.MakeConstraint(1, 1);

            for (int j = 0; j < context.ProcessSteps[i].MatchedProviders.Count; j++)
            {
                constraint.SetCoefficient(assignments[(i, j)], 1);
            }
        }
    }

    /// <summary>
    /// Builds linear expressions for total cost and total time across all selected providers.
    /// </summary>
    private static (LinearExpr totalCost,LinearExpr totalTime) BuildTotalCostAndTimeExpressions(WorkflowContext context, Dictionary<(int step, int provider), Variable> assignments)
    {
        LinearExpr totalCostExpr = new LinearExpr();
        LinearExpr totalTimeExpr = new LinearExpr();

        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];

            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var estimate = step.MatchedProviders[j].Estimate;
                totalCostExpr += (double)estimate.Cost * assignments[(i, j)];
                totalTimeExpr += estimate.Duration.TotalHours * assignments[(i, j)];
            }
        }

        return (totalCostExpr, totalTimeExpr);
    }

    /// <summary>
    /// Adds budget and deadline constraints to the solver.
    /// </summary>
    private static void AddBudgetAndDeadlineConstraints(Solver solver, WorkflowContext context, LinearExpr totalCostExpr, LinearExpr totalTimeExpr)
    {
        var constraints = context.Request.Constraints;

        // Max budget constraint
        if (constraints.MaxBudget.HasValue)
        {
            solver.Add(totalCostExpr <= (double)constraints.MaxBudget.Value);
        }

        // Deadline constraint (relative to now)
        if (constraints.RequiredDeadline.HasValue)
        {
            var maxHours = (constraints.RequiredDeadline.Value - DateTime.Now).TotalHours;
            solver.Add(totalTimeExpr <= maxHours);
        }
    }

    /// <summary>
    /// Builds objective function using weighted normalized metrics.
    /// </summary>
    private Objective BuildObjective(Solver solver, WorkflowContext context, Dictionary<(int step, int provider), Variable> assignments, OptimizationWeights weights)
    {
        var objective = solver.Objective();

        // Collect all estimates to determine normalization ranges
        var allEstimates = context.ProcessSteps
            .SelectMany(s => s.MatchedProviders)
            .Select(p => p.Estimate)
            .ToList();

        var costRange = (
            min: allEstimates.Min(e => (double)e.Cost),
            max: allEstimates.Max(e => (double)e.Cost)
        );

        var timeRange = (
            min: allEstimates.Min(e => e.Duration.TotalHours),
            max: allEstimates.Max(e => e.Duration.TotalHours)
        );

        var emissionsRange = (
            min: allEstimates.Min(e => e.EmissionsKgCO2),
            max: allEstimates.Max(e => e.EmissionsKgCO2)
        );

        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];

            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var estimate = step.MatchedProviders[j].Estimate;

                // Normalize values to 0..1 range based on actual data
                var cost = Normalize((double)estimate.Cost, costRange.min, costRange.max);
                var time = Normalize(estimate.Duration.TotalHours, timeRange.min, timeRange.max);
                var quality = estimate.QualityScore; // already 0..1
                var emissions = Normalize(estimate.EmissionsKgCO2, emissionsRange.min, emissionsRange.max);

                // Lower is better (quality is inverted)
                var coefficient =
                    weights.CostWeight * cost +
                    weights.TimeWeight * time +
                    weights.EmissionsWeight * emissions -
                    weights.QualityWeight * quality;

                objective.SetCoefficient(
                    assignments[(i, j)],
                    coefficient);
            }
        }

        return objective;
    }

    /// <summary>
    /// Normalizes a value to 0..1 range based on min and max values.
    /// </summary>
    private static double Normalize(double value, double min, double max)
    {
        if (max <= min)
            return 0;

        return (value - min) / (max - min);
    }

    /// <summary>
    /// Extracts selected providers and aggregated metrics from solver solution.
    /// </summary>
    private static OptimizationResult ExtractResult(WorkflowContext context, Dictionary<(int step, int provider), Variable> assignments, Objective objective, Solver.ResultStatus status)
    {
        decimal totalCost = 0;
        TimeSpan totalDuration = TimeSpan.Zero;
        double totalQuality = 0;
        double totalEmissions = 0;

        var selectedProviders = new Dictionary<int, MatchedProvider>();

        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];

            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                if (assignments[(i, j)].SolutionValue() <= 0.5)
                    continue;

                var provider = step.MatchedProviders[j];
                selectedProviders[step.StepNumber] = provider;

                totalCost += provider.Estimate.Cost;
                totalDuration += provider.Estimate.Duration;
                totalQuality += provider.Estimate.QualityScore;
                totalEmissions += provider.Estimate.EmissionsKgCO2;
                break;
            }
        }

        return new OptimizationResult
        {
            Metrics = new OptimizationMetricsModel
            {
                TotalCost = totalCost,
                TotalDuration = totalDuration,
                AverageQuality = totalQuality / context.ProcessSteps.Count,
                TotalEmissionsKgCO2 = totalEmissions,
                SolverStatus = status.ToString(),
                ObjectiveValue = objective.Value()
            },
            SelectedProviders = selectedProviders
        };
    }

    /// <summary>
    /// Builds a domain-level optimization strategy from calculation result.
    /// </summary>
    private OptimizationStrategyModel CreateStrategy(OptimizationPriority priority, WorkflowContext context, OptimizationResult result)
    {
        if (context.WorkflowType == null)
        {
            throw new InvalidOperationException("Cannot create strategy: WorkflowType is null.");
        }

        var (name, description) = priority.GetStrategyNameAndDescription();
        var warrantyTerms = priority.GetWarrantyTerms(context.WorkflowType);

        return new OptimizationStrategyModel
        {
            StrategyName = name,
            Priority = priority,
            WorkflowType = context.WorkflowType,
            Steps = context.ProcessSteps.Select(step =>
            {
                var provider = result.SelectedProviders[step.StepNumber];

                return new ProcessStepModel
                {
                    StepNumber = step.StepNumber,
                    Process = step.Process,
                    SelectedProviderId = provider.ProviderId,
                    SelectedProviderName = provider.ProviderName,
                    Estimate = new ProcessEstimateModel
                    {
                        Cost = provider.Estimate.Cost,
                        Duration = provider.Estimate.Duration,
                        QualityScore = provider.Estimate.QualityScore,
                        EmissionsKgCO2 = provider.Estimate.EmissionsKgCO2,
                        ProposalId = provider.Estimate.ProposalId
                    }
                };
            }).ToList(),
            Metrics = result.Metrics,
            Warranty = warrantyTerms,
            Description = description
        };
    }


    private class OptimizationResult
    {
        /// <summary>
        /// Aggregated optimization metrics.
        /// </summary>
        public OptimizationMetricsModel Metrics { get; init; } = default!;

        /// <summary>
        /// Selected provider per workflow step.
        /// Key = StepNumber
        /// </summary>
        public Dictionary<int, MatchedProvider> SelectedProviders { get; init; } = [];
    }
}