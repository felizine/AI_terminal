namespace AITerminal.Core.Conversations;

public interface IConversationRepository
{
    Task<IReadOnlyList<Conversation>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<Conversation?> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<Conversation> CreateAsync(
        string title,
        CancellationToken cancellationToken = default);

    Task UpdateTitleAsync(
        Guid conversationId,
        string title,
        CancellationToken cancellationToken = default);

    Task AddMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        Guid conversationId,
        int limit,
        CancellationToken cancellationToken = default);
}
