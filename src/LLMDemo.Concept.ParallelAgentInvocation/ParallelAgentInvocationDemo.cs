using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Models;

namespace LLMDemo.Concept.ParallelAgentInvocation;

/// <summary>
/// Demo: An orchestrator agent that fans out to sub-agents in parallel via tool-calling.
/// Ask it for multiple jokes (e.g. "tell me 5 jokes") to see parallel invocation in action.
/// </summary>
public sealed class ParallelAgentInvocationDemo : IChatConceptDemo
{
    public string Name => "Parallel Agent Invocation";
    public string Description => "Orchestrator agent that invokes sub-agents in parallel via tool-calling";
    public string? DefaultPrompt => "Tell me 5 jokes";

    private readonly IAgentRunner _orchestratorRunner;
    private readonly AgentDefinition _orchestratorAgent;

    public ParallelAgentInvocationDemo(IAgentRunner agentRunner)
    {
        _orchestratorRunner = agentRunner;

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
        // Sub-agents share the same IAgentRunner (transient; each call is independent).
        var invokeSubAgentsTool = new InvokeSubAgentsTool(agentRunner, subAgents);

        // Orchestrator agent
        _orchestratorAgent = new AgentDefinition(
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
    }

    /// <inheritdoc/>
    public Task<string> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default)
        => _orchestratorRunner.RunAsync(_orchestratorAgent, userMessage, cancellationToken);

    /// <summary>
    /// RunAsync is not used for chat demos — the CLI host calls <see cref="ProcessMessageAsync"/> in a loop.
    /// </summary>
    public Task RunAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
