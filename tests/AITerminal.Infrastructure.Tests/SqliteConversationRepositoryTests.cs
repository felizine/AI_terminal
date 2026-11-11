using AITerminal.Core.Conversations;
using AITerminal.Infrastructure;
using AITerminal.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace AITerminal.Infrastructure.Tests;

public sealed class SqliteConversationRepositoryTests
{
    [Fact]
    public async Task Repository_round_trips_conversations_and_messages()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var options = new StorageOptions(directory);
            var database = new SqliteDatabase(options);
            var repository = new SqliteConversationRepository(database);
            await database.InitializeAsync();

            var conversation = await repository.CreateAsync("Board preparation");
            var firstMessage = new ChatMessage(
                Guid.NewGuid(),
                conversation.Id,
                ChatRole.User,
                "Draft the agenda.",
                DateTimeOffset.UtcNow.AddMinutes(-2));
            var secondMessage = new ChatMessage(
                Guid.NewGuid(),
                conversation.Id,
                ChatRole.Assistant,
                "Here is a starting point.",
                DateTimeOffset.UtcNow.AddMinutes(-1));

            await repository.AddMessageAsync(firstMessage);
            await repository.AddMessageAsync(secondMessage);

            var messages = await repository.GetMessagesAsync(conversation.Id, 20);
            var recent = await repository.GetRecentAsync(5);

            Assert.Equal(conversation.Id, Assert.Single(recent).Id);
            Assert.Collection(
                messages,
                message => Assert.Equal(firstMessage.Id, message.Id),
                message => Assert.Equal(secondMessage.Id, message.Id));
            Assert.Equal(ChatRole.Assistant, messages[1].Role);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    [Fact]
    public async Task Repository_persists_data_across_database_instances()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var options = new StorageOptions(directory);
            var firstDatabase = new SqliteDatabase(options);
            var firstRepository = new SqliteConversationRepository(firstDatabase);
            await firstDatabase.InitializeAsync();
            var conversation = await firstRepository.CreateAsync("Persistent history");
            await firstRepository.AddMessageAsync(new ChatMessage(
                Guid.NewGuid(),
                conversation.Id,
                ChatRole.User,
                "Remember this.",
                DateTimeOffset.UtcNow));

            var secondDatabase = new SqliteDatabase(options);
            var secondRepository = new SqliteConversationRepository(secondDatabase);
            var restored = await secondRepository.GetAsync(conversation.Id);
            var messages = await secondRepository.GetMessagesAsync(conversation.Id, 20);

            Assert.NotNull(restored);
            Assert.Equal("Persistent history", restored.Title);
            Assert.Single(messages);
            Assert.Equal("Remember this.", messages[0].Content);
        }
        finally
        {
            DeleteDirectory(directory);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "ai-terminal-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteDirectory(string directory)
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
