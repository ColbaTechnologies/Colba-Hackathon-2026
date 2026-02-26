using WebApplication1.Models;

namespace WebApplication1.Persistence;

public interface IMessageStore
{
    Task<MessageRecord> EnqueueAsync(
        string destinationUrl,
        string? clientMessageId,
        string? serializedPayload,
        Dictionary<string, string> headers,
        string? tenantId,
        string? callbackUrl,
        DateTimeOffset? deliverAtUtc,
        CancellationToken cancellationToken);

    Task<MessageRecord?> TryGetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MessageRecord?> TryGetByClientIdAsync(string clientMessageId, string? tenantId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MessageRecord>> GetDueMessagesSnapshotAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);

    Task UpdateAsync(MessageRecord record, CancellationToken cancellationToken);

    Task<IReadOnlyList<MessageRecord>> GetActiveMessagesSnapshotAsync(string? tenantId, CancellationToken cancellationToken);
}