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
}
