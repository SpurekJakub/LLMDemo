using System.Text.Json;
using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Models;

namespace LLMDemo.Concept.ParallelAgentInvocation;

/// <summary>
/// Tool that invokes one or more sub-agents in parallel and aggregates their responses.
/// The orchestrator LLM calls this tool to fan out work across multiple agents concurrently.
/// </summary>
public sealed class InvokeSubAgentsTool : ITool
{
    private static readonly string ParametersSchema = """
        {
            "type": "object",
            "properties": {
                "requests": {
                    "type": "array",
                    "description": "List of sub-agent invocations to run in parallel.",
                    "items": {
                        "type": "object",
                        "properties": {
                            "agentName": {
                                "type": "string",
                                "description": "Name of the sub-agent to invoke."
                            },
                            "prompt": {
                                "type": "string",
                                "description": "The message/prompt to send to the sub-agent."
                            }
                        },
                        "required": ["agentName", "prompt"]
                    }
                }
            },
            "required": ["requests"]
        }
        """;

    public ToolDefinition Definition { get; } = new(
        Name: "invoke_subagents",
        Description: "Invoke one or more sub-agents in parallel. Provide an array of agent requests, each specifying the agentName and the prompt to send.",
        ParametersSchema: ParametersSchema);

    private readonly IAgentRunner _agentRunner;
    private readonly IReadOnlyDictionary<string, AgentDefinition> _subAgents;

    public InvokeSubAgentsTool(IAgentRunner agentRunner, IReadOnlyDictionary<string, AgentDefinition> subAgents)
    {
        _agentRunner = agentRunner;
        _subAgents = subAgents;
    }

    public async Task<string> InvokeAsync(string argumentsJson, CancellationToken cancellationToken = default)
    {
        // Parse the arguments
        using var doc = JsonDocument.Parse(argumentsJson);
        var requests = doc.RootElement.GetProperty("requests");

        var tasks = new List<Task<AgentResult>>();

        foreach (var request in requests.EnumerateArray())
        {
            var agentName = request.GetProperty("agentName").GetString()!;
            var prompt = request.GetProperty("prompt").GetString()!;
            tasks.Add(RunAgentAsync(agentName, prompt, cancellationToken));
        }

        var results = await Task.WhenAll(tasks);

        return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = false });
    }

    private async Task<AgentResult> RunAgentAsync(string agentName, string prompt, CancellationToken cancellationToken)
    {
        if (!_subAgents.TryGetValue(agentName, out var agentDef))
        {
            return new AgentResult(agentName, null, $"Unknown sub-agent: '{agentName}'");
        }

        try
        {
            var response = await _agentRunner.RunAsync(agentDef, prompt, cancellationToken);
            return new AgentResult(agentName, response, null);
        }
        catch (Exception ex)
        {
            return new AgentResult(agentName, null, ex.Message);
        }
    }

    private sealed record AgentResult(string AgentName, string? Response, string? Error);
}
