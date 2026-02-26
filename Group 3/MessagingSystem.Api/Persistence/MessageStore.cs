using WebApplication1.Models;

namespace WebApplication1.Persistence;

public class MessageStore : IMessageStore
{
    public Task<MessageRecord> EnqueueAsync(string destinationUrl, string? clientMessageId, string? serializedPayload, Dictionary<string, string> headers,
        string? tenantId, string? callbackUrl, DateTimeOffset? deliverAtUtc, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MessageRecord?> TryGetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MessageRecord?> TryGetByClientIdAsync(string clientMessageId, string? tenantId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MessageRecord>> GetDueMessagesSnapshotAsync(DateTimeOffset utcNow, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(MessageRecord record, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MessageRecord>> GetActiveMessagesSnapshotAsync(string? tenantId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}