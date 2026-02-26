using MessagingSystem.Application.Dtos;
using MessagingSystem.Domain.Entities;

namespace MessagingSystem.Application.Interfaces;

public interface IMessageStore
{
    Task<List<ReceivedMessage>> GetPendingMessagesAsync(DateTimeOffset utcNow, CancellationToken cancellationToken);
    
    Task MoveToRetryCollectionAsync(
        ReceivedMessage receivedMessage,
        CancellationToken ct);
    
    Task<ReceivedMessage> EnqueuePendingMessage(string destinationUrl,
        string? serializedPayload,
        Dictionary<string, string> headers,
        string? tenantId,
        string? callbackUrl,
        CancellationToken? cancellationToken);

    Task<ReceivedMessage?> TryGetByIdAsync(string id, CancellationToken cancellationToken);

    Task<ReceivedMessage?> TryGetByClientIdAsync(string clientMessageId, string? tenantId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RetryMessage>> GetDueRetriesAsync(
        DateTimeOffset now,
        CancellationToken ct);

    Task MoveToDeadLetterAsync(
        RetryMessage message,
        MessageProcessingResult result,
        CancellationToken ct);
    
    Task MarkAsCompletedAsync(ReceivedMessage receivedMessage, CancellationToken ct);
    
    Task<IReadOnlyList<MessageSummary>> GetMessageOperationsAsync(
        string originalMessageId,
        CancellationToken ct);
}