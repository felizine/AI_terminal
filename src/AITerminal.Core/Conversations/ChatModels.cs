namespace AITerminal.Core.Conversations;

public enum ChatRole
{
    User,
    Assistant,
    System,
}

public sealed record ChatMessage(
    Guid Id,
    Guid ConversationId,
    ChatRole Role,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record Conversation(
    Guid Id,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ConversationSendResult(
    ChatMessage Message,
    string ConversationTitle);
