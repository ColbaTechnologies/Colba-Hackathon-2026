using MessagingSystem.Domain.Enums;

namespace MessagingSystem.Domain.Entities;

public class ReceivedMessage
{
    public string Id { get; set; }
    public string? ClientMessageId { get; set; }
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
    
    public static string GenerateId(MessageStatus status)
    {
        var prefix = status switch
        {
            MessageStatus.Pending => "Pending",
            MessageStatus.Failed => "Failed",
            MessageStatus.Delivered => "Delivered",
            _ => "Messages"
        };

        return $"{prefix}/{Guid.NewGuid()}";
    }
}