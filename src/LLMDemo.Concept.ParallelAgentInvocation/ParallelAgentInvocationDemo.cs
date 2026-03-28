using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Models;

namespace LLMDemo.Concept.ParallelAgentInvocation;

/// <summary>
/// Demo: An orchestrator agent that fans out to sub-agents in parallel via tool-calling.
/// Ask it for multiple jokes (e.g. "tell me 5 jokes") to see parallel invocation in action.
/// </summary>
public sealed class ParallelAgentInvocationDemo : IConceptDemo
{
    public string Name => "Parallel Agent Invocation";
    public string? DefaultPrompt => "Tell me 5 jokes";

    private readonly IAgentRunner _orchestratorRunner;

    public ParallelAgentInvocationDemo(IAgentRunner agentRunner)
    {
        _orchestratorRunner = agentRunner;
    }

    /// <inheritdoc/>
    public Task<CompletionResponse> ProcessAsync(string userMessage, string model, CancellationToken ct = default)
    {
        // Sub-agent: generates a single random joke
        var jokesAgent = new AgentDefinition(
            Name: "jokes",
            SystemPrompt: "You are a comedian. Generate a single random short joke. Be creative and vary your style. Respond with only the joke — no intro, no sign-off.",
            Tools: []);

        var subAgents = new Dictionary<string, AgentDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["jokes"] = jokesAgent,
        };

        // Tool that the orchestrator uses to invoke sub-agents in parallel.
        // Model is passed so sub-agent calls use the same model.
        var invokeSubAgentsTool = new InvokeSubAgentsTool(_orchestratorRunner, subAgents, model);

        // Orchestrator agent
        var orchestratorAgent = new AgentDefinition(
            Name: "orchestrator",
            SystemPrompt: """
                You are an orchestrator agent. You have access to sub-agents that you can invoke
                using the `invoke_subagents` tool. Before executing, create a plan: determine which
                agents to call, how many times, and which can run in parallel in the same batch.

                Available sub-agents:
                - **jokes** — generates a single random short joke

                When asked for multiple items (e.g. "tell me 5 jokes"), invoke the appropriate
                sub-agent multiple times in a single parallel batch by including multiple requests
                in the `requests` array. Present the collected results to the user in a clear,
                numbered list.
                """,
            Tools: [invokeSubAgentsTool]);

        return _orchestratorRunner.RunAsync(orchestratorAgent, userMessage, model, ct);
    }
}
