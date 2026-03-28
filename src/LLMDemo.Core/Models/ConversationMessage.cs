namespace LLMDemo.Core.Models;

/// <summary>
/// Provider-agnostic message role.
/// </summary>
public enum MessageRole
{
    System,
    User,
    Assistant,
    Tool
}

/// <summary>
/// Represents a single message in a conversation.
/// </summary>
/// <param name="Role">The role of the speaker.</param>
/// <param name="Content">Text content (may be empty for pure tool-call assistant turns).</param>
/// <param name="ToolCalls">Populated for <see cref="MessageRole.Assistant"/> messages that contain tool-call requests.</param>
/// <param name="ToolCallId">Populated for <see cref="MessageRole.Tool"/> messages identifying which call this result belongs to.</param>
public sealed record ConversationMessage(
    MessageRole Role,
    string Content,
    IReadOnlyList<ToolCallInfo>? ToolCalls = null,
    string? ToolCallId = null);
