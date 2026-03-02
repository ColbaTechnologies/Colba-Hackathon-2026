namespace UQ.Api.Domain.Partial;

public class MinimalMessageData
{
    public string Id { get; set; }
    public string PublicId { get; set; }
    public string DestinationUrl { get; set; }
    public MessageState State { get; set; }
    public DateTime? ScheduledOn { get; set; }
    public string? CallbackUrl { get; set; }
    public string? CallerRequestId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MinimalMessageToRetryData : MinimalMessageData
{
    public int RetryCount { get; set; }
}