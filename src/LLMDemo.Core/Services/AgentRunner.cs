using System.Diagnostics;
using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LLMDemo.Core.Services;

/// <summary>
/// Runs a complete agent loop: calls the LLM, handles tool-call turns,
/// and returns the final aggregated <see cref="CompletionResponse"/>. Capped at 10 iterations to prevent infinite loops.
/// </summary>
/// <remarks>
/// A shared collector is stored in an <see cref="AsyncLocal{T}"/> so that any sub-agent calls
/// triggered by tool invocations during the root run append their metrics into the same list.
/// The root <see cref="RunAsync"/> call owns the collector and returns it as
/// <see cref="CompletionResponse.Steps"/>; nested calls return an empty Steps list.
/// </remarks>
public sealed class AgentRunner : IAgentRunner
{
    private const int MaxIterations = 10;

    private readonly IChatCompletionService _chatService;
    private readonly ILogger<AgentRunner> _logger;

    /// <summary>
    /// Holds the shared metrics collector for the duration of a root RunAsync call.
    /// Sub-agent calls running on child async contexts see the same list via AsyncLocal value-copy
    /// semantics — we use a reference type (List) so mutations are visible across contexts.
    /// </summary>
    private readonly AsyncLocal<List<CompletionMetrics>?> _rootCollector = new();

    public AgentRunner(IChatCompletionService chatService, ILogger<AgentRunner> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<CompletionResponse> RunAsync(
        AgentDefinition agent,
        string userMessage,
        string model,
        CancellationToken cancellationToken = default)
    {
        // Determine whether this is the root call or a nested sub-agent call.
        var isRoot = _rootCollector.Value is null;
        if (isRoot)
            _rootCollector.Value = [];

        // Safe — we just set it if isRoot, or it was already set by the root context.
        var collector = _rootCollector.Value!;

        var conversation = new List<ConversationMessage>
        {
            new(MessageRole.System, agent.SystemPrompt),
            new(MessageRole.User, userMessage),
        };

        var tools = agent.Tools.Select(t => t.Definition).ToList();
        var totalSw = Stopwatch.StartNew();

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            _logger.LogDebug(
                "[{Agent}] Iteration {Iteration}: sending {MessageCount} messages, {ToolCount} tools",
                agent.Name, iteration + 1, conversation.Count, tools.Count);

            var result = await _chatService.CompleteAsync(conversation, tools, model, cancellationToken);

            if (result.Metrics is not null)
            {
                // Tag with the agent name and add to the shared collector.
                var taggedMetrics = result.Metrics with { AgentName = agent.Name };
                lock (collector)
                    collector.Add(taggedMetrics);
            }

            if (!result.IsToolCall)
            {
                // Final text response — done.
                totalSw.Stop();

                if (isRoot)
                {
                    // Snapshot and clear so re-use of this AgentRunner in a new request starts fresh.
                    List<CompletionMetrics> steps;
                    lock (collector)
                        steps = [.. collector];
                    _rootCollector.Value = null;
                    return new CompletionResponse(result.Text ?? string.Empty, totalSw.Elapsed, steps);
                }

                return new CompletionResponse(result.Text ?? string.Empty, totalSw.Elapsed, []);
            }

            // Add the assistant message with tool calls to the conversation.
            conversation.Add(new ConversationMessage(
                MessageRole.Assistant,
                Content: string.Empty,
                ToolCalls: result.ToolCalls));

            // Execute each tool call and add results.
            foreach (var toolCall in result.ToolCalls!)
            {
                var tool = agent.Tools.FirstOrDefault(t => t.Definition.Name == toolCall.Name);
                string toolResult;

                if (tool is null)
                {
                    _logger.LogWarning("[{Agent}] Unknown tool requested: {ToolName}", agent.Name, toolCall.Name);
                    toolResult = $"{{\"error\": \"Unknown tool: {toolCall.Name}\"}}";
                }
                else
                {
                    _logger.LogDebug("[{Agent}] Invoking tool {ToolName}", agent.Name, toolCall.Name);
                    try
                    {
                        toolResult = await tool.InvokeAsync(toolCall.ArgumentsJson, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[{Agent}] Tool {ToolName} threw an exception", agent.Name, toolCall.Name);
                        toolResult = $"{{\"error\": \"{ex.Message}\"}}";
                    }
                }

                conversation.Add(new ConversationMessage(
                    MessageRole.Tool,
                    Content: toolResult,
                    ToolCallId: toolCall.Id));
            }
        }

        totalSw.Stop();
        _logger.LogWarning("[{Agent}] Reached max iterations ({Max}) without a text response", agent.Name, MaxIterations);

        if (isRoot)
        {
            List<CompletionMetrics> steps;
            lock (collector)
                steps = [.. collector];
            _rootCollector.Value = null;
            return new CompletionResponse(
                "[Agent reached maximum tool-call iterations without producing a final response.]",
                totalSw.Elapsed,
                steps);
        }

        return new CompletionResponse(
            "[Agent reached maximum tool-call iterations without producing a final response.]",
            totalSw.Elapsed,
            []);
    }
}
