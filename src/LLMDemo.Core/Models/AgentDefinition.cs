using LLMDemo.Core.Abstractions;

namespace LLMDemo.Core.Models;

/// <summary>
/// Defines an agent: its identity, system prompt, and the tools it can call.
/// </summary>
public sealed record AgentDefinition(
    string Name,
    string SystemPrompt,
    IReadOnlyList<ITool> Tools);
