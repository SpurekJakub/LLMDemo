using LLMDemo.Core.Models;

namespace LLMDemo.Core.Abstractions;

/// <summary>
/// Contract that all concept demo projects must implement.
/// The CLI host drives the chat loop and calls <see cref="ProcessAsync"/> per user turn.
/// </summary>
public interface IConceptDemo
{
    /// <summary>Unique name shown in the CLI menu.</summary>
    string Name { get; }

    /// <summary>
    /// Optional default prompt pre-filled for the user on the first turn.
    /// Return <c>null</c> to start with an empty input.
    /// </summary>
    string? DefaultPrompt => null;

    /// <summary>
    /// Process a single user message and return an aggregated response with metrics.
    /// </summary>
    Task<CompletionResponse> ProcessAsync(string userMessage, string model, CancellationToken ct = default);
}
