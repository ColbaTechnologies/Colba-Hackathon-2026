namespace ActorBaseMessaging.Models;

/// <summary>
/// RavenDB document that persists the full state of one outbound request.
/// Document ID convention: "messages/{requestId}" — deterministic and human-readable,
/// enabling O(1) LoadAsync by ID with no index required for per-actor reads.
/// RawPayload is kept so a crashed actor can be fully recovered on restart.
/// </summary>
public sealed class MessageDocument
{
    /// <summary>"messages/{requestId}"</summary>
    public string Id { get; set; } = default!;

    public string RequestId { get; set; } = default!;
    public string TargetUrl { get; set; } = default!;

    /// <summary>Raw JSON string of the original payload; used to rebuild the actor on recovery.</summary>
    public string RawPayload { get; set; } = default!;

    public MessageState State { get; set; }
    public int RetryCount { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
