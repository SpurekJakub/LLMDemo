using System.ClientModel;
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
    private readonly ChatClient _chatClient;
    private readonly LmStudioOptions _options;
    private readonly ILogger<LmStudioChatCompletionService> _logger;

    public LmStudioChatCompletionService(IOptions<LmStudioOptions> options, ILogger<LmStudioChatCompletionService> logger)
    {
        _options = options.Value;
        _logger = logger;

        // LM Studio doesn't require an API key — use a dummy value.
        var credential = new ApiKeyCredential("lm-studio");
        var clientOptions = new OpenAIClientOptions { Endpoint = _options.Endpoint };
        var openAiClient = new OpenAIClient(credential, clientOptions);

        var modelId = _options.DefaultModel ?? "default";
        _chatClient = openAiClient.GetChatClient(modelId);
    }

    public async Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        string? model = null,
        CancellationToken cancellationToken = default)
    {
        var chatMessages = new List<ChatMessage>(messages.Count);
        foreach (var msg in messages)
        {
            chatMessages.Add(msg.Role switch
            {
                MessageRole.System => ChatMessage.CreateSystemMessage(msg.Content),
                MessageRole.User => ChatMessage.CreateUserMessage(msg.Content),
                MessageRole.Assistant => ChatMessage.CreateAssistantMessage(msg.Content),
                _ => ChatMessage.CreateUserMessage(msg.Content),
            });
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
_logger.LogDebug("Sending {Count} messages to LM Studio (model: {Model})", messages.Count, model ?? _options.DefaultModel ?? "default");

        ChatCompletion completion = await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);

        return new CompletionResult(completion.Content[0].Text, new List<ToolCallInfo>());
    }
}
