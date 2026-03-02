namespace ActorBaseMessaging.Infrastructure.Persistence;

using Domain.Entities;
using Domain.Enums;

/// <summary>
/// RavenDB document that persists the full state of one outbound request.
/// Document ID convention: "messages/{requestId}" — deterministic and human-readable,
/// enabling O(1) LoadAsync by ID with no index required for per-actor reads.
/// </summary>
internal sealed class MessageDocument
{
    public string       Id          { get; set; } = default!;
    public string       RequestId   { get; set; } = default!;
    public string       TargetUrl   { get; set; } = default!;
    public string       RawPayload  { get; set; } = default!;
    public MessageState State       { get; set; }
    public int          RetryCount  { get; set; }
    public DateTime     ReceivedAt  { get; set; }
    public DateTime?    DeliveredAt { get; set; }

    public static MessageDocument FromEntity(MessageRequest entity) => new()
    {
        Id          = $"messages/{entity.Id}",
        RequestId   = entity.Id,
        TargetUrl   = entity.TargetUrl,
        RawPayload  = entity.RawPayload,
        State       = entity.State,
        RetryCount  = entity.RetryCount,
        ReceivedAt  = entity.ReceivedAt,
        DeliveredAt = entity.DeliveredAt,
    };

    public MessageRequest ToEntity() =>
        MessageRequest.Restore(
            id:          RequestId,
            targetUrl:   TargetUrl,
            rawPayload:  RawPayload,
            state:       State,
            retryCount:  RetryCount,
            receivedAt:  ReceivedAt,
            deliveredAt: DeliveredAt);
}
