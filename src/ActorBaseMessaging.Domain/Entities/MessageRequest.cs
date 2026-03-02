namespace ActorBaseMessaging.Domain.Entities;

using Enums;

/// <summary>
/// Domain entity representing a single outbound message request.
/// Encapsulates all state and state-transition behaviour.
/// </summary>
public sealed class MessageRequest
{
    public string       Id          { get; private set; } = default!;
    public string       TargetUrl   { get; private set; } = default!;
    public string       RawPayload  { get; private set; } = default!;
    public MessageState State       { get; private set; }
    public int          RetryCount  { get; private set; }
    public DateTime     ReceivedAt  { get; private set; }
    public DateTime?    DeliveredAt { get; private set; }

    private MessageRequest() { }

    // ── Factories ─────────────────────────────────────────────────────────────

    public static MessageRequest Create(string id, string targetUrl, string rawPayload) =>
        new()
        {
            Id         = id,
            TargetUrl  = targetUrl,
            RawPayload = rawPayload,
            State      = MessageState.Pending,
            RetryCount = 0,
            ReceivedAt = DateTime.UtcNow,
        };

    public static MessageRequest Restore(
        string       id,
        string       targetUrl,
        string       rawPayload,
        MessageState state,
        int          retryCount,
        DateTime     receivedAt,
        DateTime?    deliveredAt) =>
        new()
        {
            Id          = id,
            TargetUrl   = targetUrl,
            RawPayload  = rawPayload,
            State       = state,
            RetryCount  = retryCount,
            ReceivedAt  = receivedAt,
            DeliveredAt = deliveredAt,
        };

    // ── Computed properties ───────────────────────────────────────────────────

    public bool IsInProgress => State is MessageState.Pending or MessageState.Retrying;

    public bool CanRetry(int max) => RetryCount < max;

    // ── State transitions ─────────────────────────────────────────────────────

    public void MarkDelivered(DateTime at)
    {
        State       = MessageState.Delivered;
        DeliveredAt = at;
    }

    public void MarkRetrying()
    {
        RetryCount++;
        State = MessageState.Retrying;
    }

    public void MarkErroneous()
    {
        State = MessageState.Erroneous;
    }
}
