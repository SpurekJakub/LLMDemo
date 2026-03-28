namespace LLMDemo.Core.Models;

/// <summary>
/// Lightweight wrapper around a chat completion result.
/// Either <see cref="Text"/> is set (normal reply) or <see cref="ToolCalls"/> is populated (tool-calling turn).
/// </summary>
public sealed record CompletionResult(
    string? Text,
    IReadOnlyList<ToolCallInfo>? ToolCalls = null,
    CompletionMetrics? Metrics = null)
{
    /// <summary>True when the LLM responded with one or more tool calls instead of text.</summary>
    public bool IsToolCall => ToolCalls is { Count: > 0 };
}

