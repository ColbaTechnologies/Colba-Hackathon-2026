namespace WebApplication1.Models;

public sealed class MessageRecord
{
    public Guid Id { get; init; }
    public string? ClientMessageId { get; init; }
    public string DestinationUrl { get; init; } = default!;
    public string? SerializedPayload { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new();
    public string? TenantId { get; init; }
    public string? CallbackUrl { get; init; }

    public MessageStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? LastAttemptAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public string? LastError { get; set; }

    public DateTimeOffset? DeliverAtUtc { get; init; }
}