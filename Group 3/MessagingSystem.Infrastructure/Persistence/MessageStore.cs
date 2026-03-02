using MessagingSystem.Application.Configuration;
using MessagingSystem.Application.Dtos;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Domain.Entities;
using MessagingSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.CompareExchange;
using Raven.Client.Documents.Session;
using Raven.Client.Exceptions;

namespace MessagingSystem.Infrastructure.Persistence;

public class MessageStore(IDocumentStore store, IOptions<RetrySettings> settings, ILogger<MessageStore> logger) : IMessageStore
{
    public async Task<List<ReceivedMessage>> GetPendingMessagesAsync(
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        using var session = store.OpenAsyncSession();
        var now = DateTimeOffset.UtcNow;
        var results = await session.Query<ReceivedMessage>()
            .Where(x => x.Status == MessageStatus.Pending)
            .OrderBy(x => x.NextAttemptAtUtc)
            .Take(100) // snapshot control
            .ToListAsync(cancellationToken);

        return results;
    }

    public async Task<ReceivedMessage> EnqueuePendingMessage(string destinationUrl,
        string? serializedPayload,
        Dictionary<string, string> headers,
        string? tenantId,
        string? callbackUrl,
        CancellationToken? cancellationToken)
    {
        var record = new ReceivedMessage
        {
            Id = ReceivedMessage.GenerateId(MessageStatus.Pending),
            DestinationUrl = destinationUrl,
            ClientMessageId = Guid.NewGuid().ToString(),
            SerializedPayload = serializedPayload,
            Headers = headers ?? new(),
            TenantId = tenantId,
            CallbackUrl = callbackUrl,
            Status = MessageStatus.Pending,
            AttemptCount = 0,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            DeliverAtUtc = null,
            NextAttemptAtUtc = null
        };

        using var session = store.OpenAsyncSession();
        await session.StoreAsync(record, record.Id, cancellationToken ?? CancellationToken.None);
        await session.SaveChangesAsync(cancellationToken  ?? CancellationToken.None);

        return record;
    }
    
     public async Task<bool> TryClaimMessageForProcessingAsync(
        string messageId,
        string instanceId,
        CancellationToken cancellationToken)
    {
        var lockKey = $"locks/messages/{messageId}";
        
        try
        {
            var getOp = new GetCompareExchangeValueOperation<string>(lockKey);
            var existingLock = await store.Operations.SendAsync(
                getOp,
                sessionInfo: null,
                token: cancellationToken);

            if (existingLock != null)
            {
                return existingLock.Value == instanceId;
            }

            var putOp = new PutCompareExchangeValueOperation<string>(
                lockKey, 
                instanceId, 
                0);

            var result = await store.Operations.SendAsync(
                putOp,
                sessionInfo: null,
                token: cancellationToken);

            return !result.Successful;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error claiming message {MessageId}", messageId);
            return false;
        }
    }

    public async Task MarkAsCompletedAsync(ReceivedMessage message, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var dead = new ProcessedMessage
        {
            Id = "Processed/" + message.Id.Split("/").LastOrDefault(),
            OriginalMessageId = message.ClientMessageId,
            Payload = message.SerializedPayload,
            FinalAttemptCount = message.AttemptCount,
            SuccessAt = DateTimeOffset.UtcNow
        };
        await session.StoreAsync(dead, ct);
        session.Delete(message.Id);
        await session.SaveChangesAsync(ct);
    }
    
    public async Task UpdateRetries(string messageId, int retries, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var item = await session.LoadAsync<RetryMessage>(messageId, ct);
        item.AttemptCount = retries;
        await session.StoreAsync(item, ct);
        await session.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<MessageSummary>>
        GetMessageOperationsAsync(
            string originalMessageId,
            CancellationToken ct)
    {
        
        // TODO make a multi-index on ravendb
        
        using var session = store.OpenAsyncSession();

        var summaries = new List<MessageSummary>();

        var pending = await session
            .Query<ReceivedMessage>()
            .Where(x => x.Id == originalMessageId || x.ClientMessageId == originalMessageId)
            .ToListAsync(ct);

        summaries.AddRange(pending.Select(x => new MessageSummary
        {
            MessageId = x.Id,
            DestinationUrl = x.DestinationUrl,
            Status = x.Status,
            AttemptCount = x.AttemptCount,
            CreatedAtUtc = x.CreatedAtUtc,
            LastAttemptAtUtc = x.LastAttemptAtUtc,
            NextAttemptAtUtc = x.NextAttemptAtUtc,
            LastError = x.LastError,
            TenantId = x.TenantId
        }));

        var processed = await session
            .Query<ProcessedMessage>()
            .Where(x => x.OriginalMessageId == originalMessageId)
            .ToListAsync(ct);

        summaries.AddRange(processed.Select(x => new MessageSummary
        {
            MessageId = x.Id,
            Status = MessageStatus.Delivered,
            AttemptCount = x.FinalAttemptCount,
            CreatedAtUtc = x.SuccessAt
        }));

        var deadLetters = await session
            .Query<DeadLetterMessage>()
            .Where(x => x.OriginalMessageId == originalMessageId)
            .ToListAsync(ct);

        summaries.AddRange(deadLetters.Select(x => new MessageSummary
        {
            MessageId = x.Id,
            Status = MessageStatus.Failed,
            AttemptCount = x.FinalAttemptCount,
            CreatedAtUtc = x.FailedAt,
            LastAttemptAtUtc = x.FailedAt,
            LastError = x.LastError
        }));

        return summaries
            .OrderBy(x => x.CreatedAtUtc)
            .ToList();
    }

    public async Task MoveToRetryCollectionAsync(
        ReceivedMessage message,
        CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var retry = new RetryMessage
        {
            Id = "Retry/" + message.Id.Split("/").LastOrDefault(),
            OriginalMessageId = message.ClientMessageId,
            Payload = message.SerializedPayload,
            AttemptCount = message.AttemptCount,
            NextRetryAt = DateTimeOffset.UtcNow.AddSeconds(settings.Value.Delay.TotalSeconds),
            OriginalMessage = message
        };
        await session.StoreAsync(retry, ct);
        session.Delete(message.Id);
        await session.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RetryMessage>> GetDueRetriesAsync(DateTimeOffset now, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var results = await session.Query<RetryMessage>()
            .OrderBy(x => x.NextRetryAt)
            .Take(100) 
            .ToListAsync(ct);
        return results;
    }

    public async Task MoveToDeadLetterAsync(RetryMessage message, MessageProcessingResult result, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var retry = new DeadLetterMessage()
        {
            Id = "DeadLetter/" + message.Id.Split("/").LastOrDefault(),
            OriginalMessageId = message.OriginalMessageId,
            Payload = message.OriginalMessage.SerializedPayload!,
            FinalAttemptCount = message.AttemptCount,
            LastError = result.LastError,
            FailedAt = DateTimeOffset.UtcNow
        };
        await session.StoreAsync(retry, ct);
        session.Delete(message.Id);
        await session.SaveChangesAsync(ct);
    }
  
    public async Task<ReceivedMessage?> TryGetByIdAsync(string id, CancellationToken cancellationToken)
    {
        using var session = store.OpenAsyncSession();
        return await session.LoadAsync<ReceivedMessage>(id, cancellationToken);
    }

    public async Task<ReceivedMessage?> TryGetByClientIdAsync(
        string clientMessageId,
        string? tenantId,
        CancellationToken cancellationToken)
    {
         using var session = store.OpenAsyncSession();
        var query = session.Query<ReceivedMessage>()
            .Where(x => x.ClientMessageId == clientMessageId);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(x => x.TenantId == tenantId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}