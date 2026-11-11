using System.Runtime.CompilerServices;
using AITerminal.Core.Conversations;

namespace AITerminal.Infrastructure.Services;

/// <summary>
/// Phase 1 shell responder. It deliberately does not pretend to be a model.
/// Phase 2 replaces this implementation with streaming provider adapters.
/// </summary>
public sealed class OfflineChatCompletionService : IChatCompletionService
{
    private const string Message =
        "Phase 1 is connected to the local application shell. I can save this conversation, " +
        "but no language model is configured yet. Open Today to explore the adaptive workspace. " +
        "Provider connections, live chat, and Google data arrive in Phase 2.";

    public string DisplayName => "Offline shell";

    public async IAsyncEnumerable<string> StreamAsync(
        IReadOnlyList<ChatMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var word in Message.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return $"{word} ";

            try
            {
                await Task.Delay(18, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }
    }
}
