# Optimization Step — Strategy Generation with Google OR-Tools

_(User Story: [US-13](project-overview.md#epic-3-optimization--matchmaking) — Step assignment optimization)_

[OptimizationStep](../ManufacturingOptimization.Engine/Services/Pipeline/OptimizationStep.cs) — key pipeline step that generates multiple manufacturing process optimization strategies using the Google OR-Tools library to solve a linear programming problem.

## Problem Statement

For each manufacturing step (e.g., Cleaning, Disassembly, Redesign), several suitable providers with their estimates (cost, time, quality, CO₂ emissions) were selected in previous stages.

**Task:** select **one** provider for each step to minimize the objective function considering customer priority.

## Four Strategy Generation

The step generates **4 strategies** with different priorities:

1. **LowestCost** (Budget Strategy) — minimum cost
2. **FastestDelivery** (Express Strategy) — minimum time
3. **HighestQuality** (Premium Strategy) — maximum quality
4. **LowestEmissions** (Eco Strategy) — minimum CO₂ emissions

For each priority:
1. A copy of process and provider data is created
2. Optimization runs with corresponding weights
3. Strategy is formed with selected providers

## Priority Weights

Different weights are used for objective function components for each priority:

| Priority | Cost Weight | Time Weight | Quality Weight | Emissions Weight |
|-----------|-------------|-------------|----------------|------------------|
| **LowestCost** | 0.8 | 0.1 | 0.05 | 0.05 |
| **FastestDelivery** | 0.1 | 0.8 | 0.05 | 0.05 |
| **HighestQuality** | 0.2 | 0.2 | 0.5 | 0.1 |
| **LowestEmissions** | 0.1 | 0.1 | 0.2 | 0.6 |

## Linear Programming Problem Formulation

### Decision Variables

A binary variable is created for each (process step, provider) pair:

```
x[i,j] = 1, if provider j is assigned to step i
x[i,j] = 0, otherwise
```

For example, for Upgrade process (8 steps) with 2-3 providers per step, ~20 variables are created.

### Constraints

**Each step must have exactly one provider:**

```
Σ x[i,j] = 1  for each step i
j
```

This ensures that each process is executed by exactly one provider.

### Objective Function

Minimize weighted sum:

```
minimize: Σ Σ x[i,j] × (
          i j
              w_cost × normalized_cost[i,j] +
              w_time × normalized_time[i,j] +
              w_emissions × normalized_emissions[i,j] -
              w_quality × normalized_quality[i,j]
          )
```

**Normalization:** values are dynamically scaled to 0-1 range based on actual min/max values from all provider estimates:

For each metric, we calculate ranges:
- Cost range: `(min: allEstimates.Min(e => e.Cost), max: allEstimates.Max(e => e.Cost))`
- Time range: `(min: allEstimates.Min(e => e.Duration.TotalHours), max: allEstimates.Max(e => e.Duration.TotalHours))`
- Emissions range: `(min: allEstimates.Min(e => e.EmissionsKgCO2), max: allEstimates.Max(e => e.EmissionsKgCO2))`

Then normalize each value:
```csharp
double Normalize(double value, double min, double max)
{
    if (max <= min) return 0;
    return (value - min) / (max - min);
}
```

Applied normalization:
- Cost: `Normalize(estimate.Cost, costRange.min, costRange.max)`
- Time: `Normalize(estimate.Duration.TotalHours, timeRange.min, timeRange.max)`
- Quality: already in 0-1 range
- Emissions: `Normalize(estimate.EmissionsKgCO2, emissionsRange.min, emissionsRange.max)`

This approach ensures fair comparison regardless of the actual value ranges in the current optimization request, making the optimization scale-independent and more robust.

**Quality:** used with minus sign to **maximize** quality while minimizing overall function.

## Solution with Google OR-Tools

### 1. Solver Creation

```csharp
Solver solver = Solver.CreateSolver("SCIP");
```

**SCIP** (Solving Constraint Integer Programs) solver is used — a powerful open-source solver for mixed-integer programming problems.

### 2. Variable Creation

```csharp
var assignments = new Dictionary<(int stepIdx, int providerIdx), Variable>();

for (int i = 0; i < processSteps.Count; i++)
    for (int j = 0; j < step.MatchedProviders.Count; j++)
        assignments[(i, j)] = solver.MakeBoolVar($"x_{i}_{j}");
```

### 3. Adding Constraints

```csharp
for (int i = 0; i < processSteps.Count; i++)
{
    var constraint = solver.MakeConstraint(1, 1, $"one_provider_step_{i}");
    for (int j = 0; j < step.MatchedProviders.Count; j++)
        constraint.SetCoefficient(assignments[(i, j)], 1);
}
```

### 4. Objective Function Setup

```csharp
var objective = solver.Objective();

for each (step, provider):
    coefficient = 
        weights.CostWeight × normalizedCost +
        weights.TimeWeight × normalizedTime +
        weights.EmissionsWeight × normalizedEmissions -
        weights.QualityWeight × normalizedQuality;
    
    objective.SetCoefficient(assignments[(i, j)], coefficient);

objective.SetMinimization();
```

### 5. Solving

```csharp
var status = solver.Solve();

if (status == OPTIMAL || status == FEASIBLE)
{
    // Extract solution
    for each variable x[i,j]:
        if (assignments[(i, j)].SolutionValue() > 0.5)
            // Provider j selected for step i
}
```

## Extracting Results

After solving:

1. **Identify selected providers:** check value of each variable x[i,j]
2. **Calculate strategy metrics:**
   - `TotalCost` — sum of selected provider costs
   - `TotalDuration` — sum of execution times
   - `AverageQuality` — average quality score
   - `TotalEmissionsKgCO2` — sum of emissions
3. **Create OptimizationStrategy** with selected providers for each step

## Filtering Strategies by Constraints

After generating all strategies, customer constraints are applied:

**MaxBudget** — if maximum budget specified:
```csharp
strategies = strategies.Where(s => s.Metrics.TotalCost <= MaxBudget)
```

**RequiredDeadline** — if deadline specified:
```csharp
maxAllowedHours = (RequiredDeadline - DateTime.Now).TotalHours
strategies = strategies.Where(s => s.Metrics.TotalDuration.TotalHours <= maxAllowedHours)
```

If strategies remain after filtering, only they are returned. If all strategies are filtered out, originals are returned (so customer knows their constraints are unfeasible).

## Additional Strategy Parameters

For each strategy the following are also defined:

### Warranty and Insurance

Depending on priority and workflow type (Upgrade/Refurbish):

| Priority | Upgrade Warranty | Upgrade Insurance | Refurbish Warranty | Refurbish Insurance |
|-----------|------------------|-------------------|--------------------|--------------------|
| **HighestQuality** | Platinum 3 Years | ✓ | Gold 18 Months | ✓ |
| **FastestDelivery** | Gold 12 Months | ✓ | Silver 6 Months | ✗ |
| **LowestEmissions** | Gold 12 Months | ✓ | Silver 9 Months | ✓ |
| **LowestCost** | Basic 3 Months | ✗ | Basic 3 Months | ✗ |

### Strategy Description

Each strategy is assigned:
- **Name:** Budget Strategy, Express Strategy, Premium Strategy, Eco Strategy
- **Description:** brief explanation for customer

## Example Workflow

**Input:**
- 8 Upgrade process steps
- For each step, 2-3 suitable providers
- Each provider has estimates (cost, time, quality, emissions)

**For LowestCost priority:**
1. Variables created: x[0,0], x[0,1], ..., x[7,2] (~20 variables)
2. Constraints added: each step = 1 provider
3. Objective function minimizes cost (weight 0.8)
4. Solver selects cheapest providers for each step
5. "Budget Strategy" formed with total cost ~€3500, time ~120 hours

**For FastestDelivery priority:**
1. Same variables and constraints
2. Objective function minimizes time (weight 0.8)
3. Solver selects fastest providers
4. "Express Strategy" formed with time ~80 hours, cost ~€5200

**Result:** 4 strategies with different trade-offs between cost, time, quality, and emissions.

## Approach Benefits

1. **Optimality** — OR-Tools guarantees finding optimal solution
2. **Flexibility** — easy to add new priorities or change weights
3. **Scalability** — algorithm efficiently handles dozens of providers
4. **Transparency** — customer sees multiple options and can choose suitable one
5. **Constraint handling** — automatic filtering by budget and deadlines
