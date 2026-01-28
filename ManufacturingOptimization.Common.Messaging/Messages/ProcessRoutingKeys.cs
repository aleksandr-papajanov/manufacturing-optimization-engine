namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class ProcessRoutingKeys
{
    public const string Propose = "process.propose";
    public const string Accepted = "process.accepted";
    public const string Declined = "process.declined";
    public const string Execute = "process.execute"; 
    public const string ExecutionCompleted = "process.execution.completed";
    public const string ExecutionFailed = "process.execution.failed";
}
