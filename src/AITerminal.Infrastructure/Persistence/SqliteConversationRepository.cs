using System.Globalization;
using AITerminal.Core.Conversations;
using Microsoft.Data.Sqlite;

namespace AITerminal.Infrastructure.Persistence;

public sealed class SqliteConversationRepository(SqliteDatabase database)
    : IConversationRepository
{
    public async Task<IReadOnlyList<Conversation>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, title, created_at, updated_at
            FROM conversations
            ORDER BY updated_at DESC
            LIMIT $limit;
            """;
        command.Parameters.AddWithValue("$limit", ClampLimit(limit));

        var conversations = new List<Conversation>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            conversations.Add(ReadConversation(reader));
        }

        return conversations;
    }

    public async Task<Conversation?> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, title, created_at, updated_at
            FROM conversations
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", conversationId.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? ReadConversation(reader)
            : null;
    }

    public async Task<Conversation> CreateAsync(
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var conversation = new Conversation(
            Guid.NewGuid(),
            title.Trim(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);

        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO conversations (id, title, created_at, updated_at)
            VALUES ($id, $title, $createdAt, $updatedAt);
            """;
        command.Parameters.AddWithValue("$id", conversation.Id.ToString("D"));
        command.Parameters.AddWithValue("$title", conversation.Title);
        command.Parameters.AddWithValue("$createdAt", Format(conversation.CreatedAt));
        command.Parameters.AddWithValue("$updatedAt", Format(conversation.UpdatedAt));

        await command.ExecuteNonQueryAsync(cancellationToken);
        return conversation;
    }

    public async Task UpdateTitleAsync(
        Guid conversationId,
        string title,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE conversations
            SET title = $title, updated_at = $updatedAt
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", conversationId.ToString("D"));
        command.Parameters.AddWithValue("$title", title.Trim());
        command.Parameters.AddWithValue("$updatedAt", Format(DateTimeOffset.UtcNow));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText =
                """
                INSERT INTO chat_messages (id, conversation_id, role, content, created_at)
                VALUES ($id, $conversationId, $role, $content, $createdAt);
                """;
            command.Parameters.AddWithValue("$id", message.Id.ToString("D"));
            command.Parameters.AddWithValue("$conversationId", message.ConversationId.ToString("D"));
            command.Parameters.AddWithValue("$role", message.Role.ToString());
            command.Parameters.AddWithValue("$content", message.Content);
            command.Parameters.AddWithValue("$createdAt", Format(message.CreatedAt));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText =
                """
                UPDATE conversations
                SET updated_at = $updatedAt
                WHERE id = $conversationId;
                """;
            command.Parameters.AddWithValue("$updatedAt", Format(message.CreatedAt));
            command.Parameters.AddWithValue("$conversationId", message.ConversationId.ToString("D"));

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        transaction.Commit();
    }

    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(
        Guid conversationId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await database.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, conversation_id, role, content, created_at
            FROM (
                SELECT id, conversation_id, role, content, created_at
                FROM chat_messages
                WHERE conversation_id = $conversationId
                ORDER BY created_at DESC
                LIMIT $limit
            )
            ORDER BY created_at ASC;
            """;
        command.Parameters.AddWithValue("$conversationId", conversationId.ToString("D"));
        command.Parameters.AddWithValue("$limit", ClampLimit(limit));

        var messages = new List<ChatMessage>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            messages.Add(new ChatMessage(
                Guid.Parse(reader.GetString(0)),
                Guid.Parse(reader.GetString(1)),
                Enum.Parse<ChatRole>(reader.GetString(2), ignoreCase: true),
                reader.GetString(3),
                Parse(reader.GetString(4))));
        }

        return messages;
    }

    private static Conversation ReadConversation(SqliteDataReader reader) =>
        new(
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            Parse(reader.GetString(2)),
            Parse(reader.GetString(3)));

    private static int ClampLimit(int limit) => Math.Clamp(limit, 1, 500);

    private static string Format(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset Parse(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
