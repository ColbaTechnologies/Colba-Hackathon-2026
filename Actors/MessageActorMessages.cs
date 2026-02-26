namespace ActorBaseMessaging.Actors;

using Models;

// ── Commands sent TO the actor ────────────────────────────────────────────────

/// <summary>Triggers the first HTTP delivery attempt (sent by the actor to itself on start).</summary>
public record ProcessRequest;

/// <summary>Triggers a retry attempt; sent by the scheduler after a delay.</summary>
public record RetryRequest;

/// <summary>Asks the actor to reply with its current status snapshot.</summary>
public record GetStatus;

// ── Response from the actor ───────────────────────────────────────────────────

/// <summary>Snapshot of actor state returned in response to <see cref="GetStatus"/>.</summary>
public record StatusResponse(
    string       RequestId,
    string       TargetUrl,
    MessageState State,
    int          RetryCount,
    DateTime     ReceivedAt,
    DateTime?    DeliveredAt
);
