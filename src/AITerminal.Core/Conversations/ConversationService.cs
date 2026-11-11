using System.Text;

namespace AITerminal.Core.Conversations;

public sealed class ConversationService(
    IConversationRepository repository,
    IChatCompletionService chat)
{
    private const int ContextMessageLimit = 100;
    private const int MaximumTitleLength = 58;

    public Task<IReadOnlyList<Conversation>> GetRecentAsync(
        CancellationToken cancellationToken = default) =>
        repository.GetRecentAsync(20, cancellationToken);

    public Task<Conversation> CreateAsync(CancellationToken cancellationToken = default) =>
        repository.CreateAsync("New conversation", cancellationToken);

    public Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default) =>
        repository.GetMessagesAsync(conversation.Id, ContextMessageLimit, cancellationToken);

    public async Task<ConversationSendResult> SendAsync(
        Conversation conversation,
        string content,
        IProgress<string>? streamedText = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        var normalizedContent = content.Trim();
        var now = DateTimeOffset.UtcNow;
        var userMessage = new ChatMessage(
            Guid.NewGuid(),
            conversation.Id,
            ChatRole.User,
            normalizedContent,
            now);

        await repository.AddMessageAsync(userMessage, cancellationToken);

        var title = conversation.Title;
        if (string.Equals(title, "New conversation", StringComparison.Ordinal))
        {
            title = BuildConversationTitle(normalizedContent);
            await repository.UpdateTitleAsync(conversation.Id, title, cancellationToken);
        }

        var context = await repository.GetMessagesAsync(
            conversation.Id,
            ContextMessageLimit,
            cancellationToken);

        var response = new StringBuilder();
        await foreach (var delta in chat.StreamAsync(context, cancellationToken))
        {
            if (string.IsNullOrEmpty(delta))
            {
                continue;
            }

            response.Append(delta);
            streamedText?.Report(delta);
        }

        var assistantMessage = new ChatMessage(
            Guid.NewGuid(),
            conversation.Id,
            ChatRole.Assistant,
            response.ToString().Trim(),
            DateTimeOffset.UtcNow);

        await repository.AddMessageAsync(assistantMessage, cancellationToken);

        return new ConversationSendResult(assistantMessage, title);
    }

    internal static string BuildConversationTitle(string content)
    {
        var firstLine = content
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()?
            .Trim() ?? "New conversation";

        return firstLine.Length <= MaximumTitleLength
            ? firstLine
            : $"{firstLine[..MaximumTitleLength].TrimEnd()}...";
    }
}
