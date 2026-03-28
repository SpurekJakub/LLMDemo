namespace LLMDemo.Core.Models;

/// <summary>
/// Lightweight wrapper around a chat completion result.
/// </summary>
public sealed record CompletionResult(
    string? Text,
    List<ToolCallInfo> ToolCalls = null!);

