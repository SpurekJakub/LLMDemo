namespace LLMDemo.Core.Models;

/// <summary>
/// Represents a single tool-call requested by the LLM in an assistant message.
/// </summary>
public sealed record ToolCallInfo(string Id, string Name, string ArgumentsJson, string? ResultJson = null);
