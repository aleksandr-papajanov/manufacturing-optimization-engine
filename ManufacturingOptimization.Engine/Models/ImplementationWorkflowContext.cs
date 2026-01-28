using ManufacturingOptimization.Common.Models.Data.Entities;
using System;
using System.Collections.Generic;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Context for the execution phase (Phase 2).
/// Tracks the real-time progress of an active Optimization Plan as it moves through providers.
/// </summary>
public class ImplementationWorkflowContext
{
    /// <summary>
    /// The plan entity being executed.
    /// </summary>
    public required OptimizationPlanEntity Plan { get; init; }

    /// <summary>
    /// The index of the step currently being executed (0-based).
    /// </summary>
    public int CurrentStepIndex { get; set; } = 0;

    /// <summary>
    /// ID of the command currently pending a response (for correlation).
    /// </summary>
    public Guid? CurrentCommandId { get; set; }

    /// <summary>
    /// Runtime logs specific to this execution instance.
    /// </summary>
    public List<string> ExecutionLogs { get; } = new();

    /// <summary>
    /// Indicates if the execution has been flagged for cancellation.
    /// </summary>
    public bool IsCancelled { get; set; } = false;

    /// <summary>
    /// Helper to add a log entry with timestamp.
    /// </summary>
    public void AddLog(string message)
    {
        ExecutionLogs.Add($"[{DateTime.UtcNow:O}] {message}");
    }
}