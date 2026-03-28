namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Extends <see cref="IConceptDemo"/> for demos that run as interactive chat sessions.
/// The CLI host drives the chat loop and calls <see cref="ProcessMessageAsync"/> per user turn.
/// </summary>
public interface IChatConceptDemo : IConceptDemo
{
    /// <summary>
    /// Optional default prompt pre-filled for the user on the first turn.
    /// The user can edit or clear it before submitting.
    /// Return <c>null</c> to start with an empty input.
    /// </summary>
    string? DefaultPrompt => null;

    /// <summary>
    /// Process a single user message and return the assistant's reply.
    /// </summary>
    Task<string> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default);
}
