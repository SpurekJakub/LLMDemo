using LLMDemo.Core.Models;

namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Runs a complete agent loop: sends <paramref name="userMessage"/> to the agent,
/// handles tool-calling turns until the agent produces a final text response.
/// </summary>
public interface IAgentRunner
{
    /// <summary>
    /// Execute the agent defined by <paramref name="agent"/> with the given <paramref name="userMessage"/>.
    /// Returns the agent's final text response.
    /// </summary>
    Task<string> RunAsync(
        AgentDefinition agent,
        string userMessage,
        CancellationToken cancellationToken = default);
}
