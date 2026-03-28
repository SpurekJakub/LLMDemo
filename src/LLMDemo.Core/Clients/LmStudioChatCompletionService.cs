using System.ClientModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using LLMDemo.Core.Abstractions;
using LLMDemo.Core.Configuration;
using LLMDemo.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LLMDemo.Core.Clients;

/// <summary>
/// Chat completion service backed by LM Studio (OpenAI-compatible endpoint).
/// </summary>
public sealed class LmStudioChatCompletionService : IChatCompletionService
{
    private readonly LmStudioOptions _options;
    private readonly ILogger<LmStudioChatCompletionService> _logger;
    private readonly OpenAIClient _openAiClient;
    private readonly ConcurrentDictionary<string, ChatClient> _clientCache = new(StringComparer.OrdinalIgnoreCase);

    public LmStudioChatCompletionService(IOptions<LmStudioOptions> options, ILogger<LmStudioChatCompletionService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // LM Studio doesn't require an API key — use a dummy value.
        var credential = new ApiKeyCredential("lm-studio");
        var clientOptions = new OpenAIClientOptions { Endpoint = _options.Endpoint };
        _openAiClient = new OpenAIClient(credential, clientOptions);
    }

    /// <inheritdoc/>
    public Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        string? model = null,
        CancellationToken cancellationToken = default)
        => CompleteAsync(messages, [], model, cancellationToken);

    /// <inheritdoc/>
    public async Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedModel = model ?? _options.DefaultModel
            ?? throw new InvalidOperationException(
                "No model specified. Pass a model parameter or set LmStudio:DefaultModel in appsettings.");

        var chatClient = _clientCache.GetOrAdd(resolvedModel, m => _openAiClient.GetChatClient(m));

        var chatMessages = BuildChatMessages(messages);

        _logger.LogDebug(
            "Sending {Count} messages to LM Studio (model: {Model}, tools: {ToolCount})",
            messages.Count,
            resolvedModel,
            tools.Count);

        var sw = Stopwatch.StartNew();
        ChatCompletion completion;

        if (tools.Count > 0)
        {
            var chatTools = tools.Select(t => ChatTool.CreateFunctionTool(
                t.Name,
                t.Description,
                BinaryData.FromString(t.ParametersSchema))).ToList();

            var completionOptions = new ChatCompletionOptions();
            foreach (var tool in chatTools)
                completionOptions.Tools.Add(tool);

            completion = await chatClient.CompleteChatAsync(chatMessages, completionOptions, cancellationToken);
        }
        else
        {
            completion = await chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);
        }

        sw.Stop();

        var metrics = new CompletionMetrics(
            Duration: sw.Elapsed,
            PromptTokens: completion.Usage?.InputTokenCount,
            CompletionTokens: completion.Usage?.OutputTokenCount,
            TotalTokens: completion.Usage?.TotalTokenCount);

        // Tool-call response
        if (completion.FinishReason == ChatFinishReason.ToolCalls)
        {
            var toolCalls = completion.ToolCalls
                .Select(tc => new ToolCallInfo(tc.Id, tc.FunctionName, tc.FunctionArguments.ToString()))
                .ToList();

            _logger.LogDebug("LLM requested {Count} tool call(s)", toolCalls.Count);
            return new CompletionResult(null, toolCalls, metrics);
        }

        // Normal text response
        var text = completion.Content.Count > 0 ? completion.Content[0].Text : string.Empty;
        return new CompletionResult(text, null, metrics);
    }

    private static List<ChatMessage> BuildChatMessages(IReadOnlyList<ConversationMessage> messages)
    {
        var chatMessages = new List<ChatMessage>(messages.Count);

        foreach (var msg in messages)
        {
            switch (msg.Role)
            {
                case MessageRole.System:
                    chatMessages.Add(ChatMessage.CreateSystemMessage(msg.Content));
                    break;

                case MessageRole.User:
                    chatMessages.Add(ChatMessage.CreateUserMessage(msg.Content));
                    break;

                case MessageRole.Assistant when msg.ToolCalls is { Count: > 0 }:
                    // Assistant message that requested tool calls
                    var assistantMsg = ChatMessage.CreateAssistantMessage(
                        msg.ToolCalls.Select(tc =>
                            ChatToolCall.CreateFunctionToolCall(tc.Id, tc.Name, BinaryData.FromString(tc.ArgumentsJson)))
                        .ToArray<ChatToolCall>());
                    chatMessages.Add(assistantMsg);
                    break;

                case MessageRole.Assistant:
                    chatMessages.Add(ChatMessage.CreateAssistantMessage(msg.Content));
                    break;

                case MessageRole.Tool:
                    chatMessages.Add(ChatMessage.CreateToolMessage(msg.ToolCallId!, msg.Content));
                    break;

                default:
                    chatMessages.Add(ChatMessage.CreateUserMessage(msg.Content));
                    break;
            }
        }

        return chatMessages;
    }
}
