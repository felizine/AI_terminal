using System.Runtime.CompilerServices;
using AITerminal.Core.Conversations;

namespace AITerminal.Core.Tests;

internal sealed class InMemoryConversationRepository : IConversationRepository
{
    private readonly List<Conversation> _conversations = [];
    private readonly List<ChatMessage> _messages = [];

    public IReadOnlyList<ChatMessage> Messages => _messages;

    public IReadOnlyList<Conversation> Conversations => _conversations;

    public Task<IReadOnlyList<Conversation>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Conversation>>(
            _conversations.OrderByDescending(item => item.UpdatedAt).Take(limit).ToArray());

    public Task<Conversation?> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_conversations.FirstOrDefault(item => item.Id == conversationId));

    public Task<Conversation> CreateAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation(
            Guid.NewGuid(),
            title,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
        _conversations.Add(conversation);
        return Task.FromResult(conversation);
    }

    public Task UpdateTitleAsync(
        Guid conversationId,
        string title,
        CancellationToken cancellationToken = default)
    {
        var index = _conversations.FindIndex(item => item.Id == conversationId);
        _conversations[index] = _conversations[index] with
        {
            Title = title,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        return Task.CompletedTask;
    }

    public Task AddMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
        var index = _conversations.FindIndex(item => item.Id == message.ConversationId);
        if (index >= 0)
        {
            _conversations[index] = _conversations[index] with
            {
                UpdatedAt = message.CreatedAt,
            };
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        Guid conversationId,
        int limit,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ChatMessage>>(
            _messages
                .Where(item => item.ConversationId == conversationId)
                .OrderBy(item => item.CreatedAt)
                .Take(limit)
                .ToArray());
}

internal sealed class FakeChatCompletionService(params string[] chunks)
    : IChatCompletionService
{
    public string DisplayName => "Test model";

    public async IAsyncEnumerable<string> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return chunk;
            await Task.Yield();
        }
    }
}

internal sealed class InlineProgress<T>(Action<T> handler) : IProgress<T>
{
    public void Report(T value) => handler(value);
}
