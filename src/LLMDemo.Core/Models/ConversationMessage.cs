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
public sealed record ConversationMessage(MessageRole Role, string Content);
