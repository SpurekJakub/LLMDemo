using LLMDemo.Core.Models;

namespace LLMDemo.Core.Clients;

/// <summary>
/// Abstraction over the LM Studio chat completion API.
/// </summary>
public interface ILmStudioClient
{
    /// <summary>
    /// Send a list of messages and get a completion back.
    /// </summary>
    Task<CompletionResult> CompleteAsync(
        IReadOnlyList<ConversationMessage> messages,
        string? model = null,
        CancellationToken cancellationToken = default);
}
