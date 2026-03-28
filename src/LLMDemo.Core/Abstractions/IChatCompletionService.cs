using LLMDemo.Core.Models;

namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Provider-agnostic chat completion service.
/// Implement this to add support for a new AI provider (LM Studio, OpenAI, Azure, etc.).
/// </summary>
public interface IChatCompletionService
{
    /// <summary>Send a conversation and get a completion back.</summary>
    Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        string? model = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a conversation with available tools and get a completion back.
    /// The result may contain tool-call requests instead of a text reply.
    /// </summary>
    Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        IReadOnlyList<ToolDefinition> tools,
        string? model = null,
        CancellationToken cancellationToken = default);
}
