namespace AITerminal.Core.Conversations;

public interface IChatCompletionService
{
    string DisplayName { get; }

    IAsyncEnumerable<string> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}
