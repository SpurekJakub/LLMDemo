namespace LLMDemo.Core.Models;

/// <summary>
/// Aggregated response returned by an agent run or concept demo.
/// Contains the final text, total elapsed time, and per-call metrics for each LLM step.
/// </summary>
public sealed record CompletionResponse(
    string Text,
    TimeSpan TotalDuration,
    IReadOnlyList<CompletionMetrics> Steps);
