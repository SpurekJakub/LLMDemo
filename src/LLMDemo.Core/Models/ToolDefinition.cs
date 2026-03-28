namespace LLMDemo.Core.Models;

public sealed record ToolDefinition(string Name, string Description, string ParametersJson);
