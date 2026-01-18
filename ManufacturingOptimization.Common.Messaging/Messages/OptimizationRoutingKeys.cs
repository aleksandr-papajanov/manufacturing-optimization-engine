namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class OptimizationRoutingKeys
{
    public const string PlanRequested = "optimization.plan-requested";
    public const string PlanCreated = "optimization.plan-created";
    public const string PlanReady = "optimization.plan-ready";
    
    // US-07: Multiple Strategies
    public const string StrategiesReady = "optimization.strategies-ready";
    public const string StrategySelected = "optimization.strategy.selected";
}

