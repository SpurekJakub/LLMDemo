namespace LLMDemo.Core.Models;

/// <summary>
/// Provider-agnostic tool metadata sent to the LLM.
/// <see cref="ParametersSchema"/> must be a valid JSON Schema object string.
/// </summary>
public sealed record ToolDefinition(string Name, string Description, string ParametersSchema);
