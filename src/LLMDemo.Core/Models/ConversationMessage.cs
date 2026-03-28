using OpenAI.Chat;

namespace LLMDemo.Core.Models;

/// <summary>
/// Represents a single message in a conversation.
/// </summary>
public sealed record ConversationMessage(ChatMessageRole Role, string Content);
