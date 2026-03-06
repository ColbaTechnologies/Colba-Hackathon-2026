namespace ActorBaseMessaging.Infrastructure.Persistence;

using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.CompareExchange;

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

    private const string RecoveryCxKey = "recovery/completed-at";

    public async Task MarkRecoveryCompleteAsync()
    {
        // CAS write: get current index then atomically swap in the new timestamp.
        // Retry if another writer changed the index between get and put.
        while (true)
        {
            var current = await store.Operations.SendAsync(
                new GetCompareExchangeValueOperation<DateTime?>(RecoveryCxKey));

            var result = await store.Operations.SendAsync(
                new PutCompareExchangeValueOperation<DateTime?>(
                    RecoveryCxKey,
                    DateTime.UtcNow,
                    current?.Index ?? 0));

            if (result.Successful) return;
            // Index changed — another initializer raced us; retry to ensure our timestamp wins.
        }
    }

    public async Task<DateTime?> GetRecoveryCompletedAtAsync()
    {
        var result = await store.Operations.SendAsync(
            new GetCompareExchangeValueOperation<DateTime?>(RecoveryCxKey));

        return result?.Value;
    }
}
