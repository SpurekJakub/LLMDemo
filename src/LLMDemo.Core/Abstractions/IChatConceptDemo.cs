namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Extends <see cref="IConceptDemo"/> for demos that run as interactive chat sessions.
/// The CLI host drives the chat loop and calls <see cref="ProcessMessageAsync"/> per user turn.
/// </summary>
public interface IChatConceptDemo : IConceptDemo
{
    /// <summary>
    /// Process a single user message and return the assistant's reply.
    /// </summary>
    Task<string> ProcessMessageAsync(string userMessage, CancellationToken cancellationToken = default);
}
