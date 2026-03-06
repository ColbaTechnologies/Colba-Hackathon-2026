namespace ActorBaseMessaging.Infrastructure.Persistence;

using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Raven.Client.Documents;

public sealed class RavenDbMessageRepository(IDocumentStore store) : IMessageRepository
{
    public async Task<MessageRequest?> GetByIdAsync(string id)
    {
        using var session = store.OpenAsyncSession();
        var doc = await session.LoadAsync<MessageDocument>($"messages/{id}");
        return doc?.ToEntity();
    }

    public async Task SaveAsync(MessageRequest message)
    {
        using var session = store.OpenAsyncSession();
        await session.StoreAsync(MessageDocument.FromEntity(message), $"messages/{message.Id}");
        await session.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<MessageRequest>> GetInFlightAsync()
    {
        using var session = store.OpenAsyncSession();
        var docs = await session.Query<MessageDocument>()
            .Where(d => d.State == MessageState.Pending || d.State == MessageState.Retrying)
            .ToListAsync();

        return docs.Select(d => d.ToEntity()).ToList();
    }

    public async Task MarkRecoveryCompleteAsync()
    {
        using var session = store.OpenAsyncSession();
        var doc = await session.LoadAsync<RecoveryMetadataDocument>(RecoveryMetadataDocument.DocumentId)
                  ?? new RecoveryMetadataDocument();
        doc.CompletedAt = DateTime.UtcNow;
        await session.StoreAsync(doc, RecoveryMetadataDocument.DocumentId);
        await session.SaveChangesAsync();
    }

    public async Task<DateTime?> GetRecoveryCompletedAtAsync()
    {
        using var session = store.OpenAsyncSession();
        var doc = await session.LoadAsync<RecoveryMetadataDocument>(RecoveryMetadataDocument.DocumentId);
        return doc?.CompletedAt;
    }

    private sealed class RecoveryMetadataDocument
    {
        public const string DocumentId = "recovery/metadata";
        public DateTime? CompletedAt { get; set; }
    }
}
