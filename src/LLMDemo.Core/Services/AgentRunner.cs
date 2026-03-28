using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Models;
using Microsoft.Extensions.Logging;

namespace LLMDemo.Core.Services;

/// <summary>
/// Runs a complete agent loop: calls the LLM, handles tool-call turns,
/// and returns the final text response. Capped at 10 iterations to prevent infinite loops.
/// </summary>
public sealed class AgentRunner : IAgentRunner
{
    private const int MaxIterations = 10;

    private readonly IChatCompletionService _chatService;
    private readonly ILogger<AgentRunner> _logger;

    public AgentRunner(IChatCompletionService chatService, ILogger<AgentRunner> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> RunAsync(
        AgentDefinition agent,
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var conversation = new List<ConversationMessage>
        {
            new(MessageRole.System, agent.SystemPrompt),
            new(MessageRole.User, userMessage),
        };

        var tools = agent.Tools.Select(t => t.Definition).ToList();

        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            _logger.LogDebug(
                "[{Agent}] Iteration {Iteration}: sending {MessageCount} messages, {ToolCount} tools",
                agent.Name, iteration + 1, conversation.Count, tools.Count);

            var result = await _chatService.CompleteAsync(conversation, tools, cancellationToken: cancellationToken);

            if (!result.IsToolCall)
            {
                // Final text response — done.
                return result.Text ?? string.Empty;
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

        _logger.LogWarning("[{Agent}] Reached max iterations ({Max}) without a text response", agent.Name, MaxIterations);
        return "[Agent reached maximum tool-call iterations without producing a final response.]";
    }
}
