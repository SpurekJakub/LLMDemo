using System.ClientModel;
using LLMDemo.Core.Configuration;
using LLMDemo.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LLMDemo.Core.Clients;

/// <summary>
/// LM Studio client that uses the OpenAI .NET SDK pointed at a local endpoint.
/// </summary>
public sealed class LmStudioClient : ILmStudioClient
{
    private readonly ChatClient _chatClient;
    private readonly LmStudioOptions _options;
    private readonly ILogger<LmStudioClient> _logger;

    public LmStudioClient(IOptions<LmStudioOptions> options, ILogger<LmStudioClient> logger)
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
                ChatMessageRole.System => ChatMessage.CreateSystemMessage(msg.Content),
                ChatMessageRole.User => ChatMessage.CreateUserMessage(msg.Content),
                ChatMessageRole.Assistant => ChatMessage.CreateAssistantMessage(msg.Content),
                _ => ChatMessage.CreateUserMessage(msg.Content),
            });
        }

        _logger.LogDebug("Sending {Count} messages to LM Studio (model: {Model})", messages.Count, model ?? _options.DefaultModel ?? "default");

        ChatCompletion completion = await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);

        return new CompletionResult(
            Content: completion.Content[0].Text,
            Model: completion.Model,
            PromptTokens: completion.Usage.InputTokenCount,
            CompletionTokens: completion.Usage.OutputTokenCount);
    }
}
