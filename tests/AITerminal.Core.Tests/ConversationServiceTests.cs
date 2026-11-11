using AITerminal.Core.Conversations;

namespace AITerminal.Core.Tests;

public sealed class ConversationServiceTests
{
    [Fact]
    public async Task SendAsync_persists_user_and_assistant_messages_and_streams_text()
    {
        var repository = new InMemoryConversationRepository();
        var service = new ConversationService(
            repository,
            new FakeChatCompletionService("Hello", " from ", "the test."));
        var conversation = await service.CreateAsync();
        var streamed = new System.Text.StringBuilder();

        var result = await service.SendAsync(
            conversation,
            "Plan my day.",
            new InlineProgress<string>(chunk => streamed.Append(chunk)));

        Assert.Equal("Plan my day.", result.ConversationTitle);
        Assert.Equal("Hello from the test.", result.Message.Content);
        Assert.Equal(result.Message.Content, streamed.ToString());
        Assert.Equal(2, repository.Messages.Count);
        Assert.Equal(ChatRole.User, repository.Messages[0].Role);
        Assert.Equal(ChatRole.Assistant, repository.Messages[1].Role);
    }

    [Fact]
    public async Task SendAsync_rejects_blank_content()
    {
        var service = new ConversationService(
            new InMemoryConversationRepository(),
            new FakeChatCompletionService("ignored"));
        var conversation = await service.CreateAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.SendAsync(conversation, "   "));
    }

    [Fact]
    public async Task SendAsync_generates_a_short_single_line_title()
    {
        var service = new ConversationService(
            new InMemoryConversationRepository(),
            new FakeChatCompletionService("ok"));
        var conversation = await service.CreateAsync();

        var result = await service.SendAsync(
            conversation,
            "This is a deliberately long first line that should become a concise conversation title and not retain every character\nSecond line");

        Assert.InRange(result.ConversationTitle.Length, 4, 61);
        Assert.EndsWith("...", result.ConversationTitle);
        Assert.StartsWith("This is a deliberately long first line", result.ConversationTitle);
        Assert.DoesNotContain('\n', result.ConversationTitle);
    }
}
