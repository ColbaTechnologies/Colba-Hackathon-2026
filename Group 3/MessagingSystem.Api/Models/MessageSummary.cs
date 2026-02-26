namespace WebApplication1.Models;


public sealed class MessageSummary
{
    public Guid MessageId { get; init; }
    public string DestinationUrl { get; init; } = default!;
    
    public string? TopicName { get; init; }
    
    
    public MessageStatus Status { get; init; }
    public int AttemptCount { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? LastAttemptAtUtc { get; init; }
    
    
    public DateTimeOffset? NextAttemptAtUtc { get; init; }
    
    
    public string? LastError { get; init; }
    public string? TenantId { get; init; }
}