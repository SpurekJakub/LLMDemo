namespace LLMDemo.Core.Models;

public sealed record ToolCallInfo(string Name, string ArgumentsJson, string? ResultJson = null);
