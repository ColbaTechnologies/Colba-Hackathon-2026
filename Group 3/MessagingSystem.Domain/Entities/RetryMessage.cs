namespace MessagingSystem.Domain.Entities;

public sealed class RetryMessage
{
    public string Id { get; set; } = default!;
    public string OriginalMessageId { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public int AttemptCount { get; set; }
    public DateTimeOffset NextRetryAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public ReceivedMessage OriginalMessage { get; set; }
}