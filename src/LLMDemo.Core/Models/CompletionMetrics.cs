namespace LLMDemo.Core.Models;

/// <summary>
/// Timing and token-usage metrics captured for a single LLM completion call.
/// </summary>
public sealed record CompletionMetrics(
    DateTimeOffset RequestedAt,
    DateTimeOffset RespondedAt,
    TimeSpan Duration,
    int? PromptTokens,
    int? CompletionTokens,
    int? TotalTokens);
