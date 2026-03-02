namespace MessagingSystem.Domain.Entities;

public class ProcessedMessage
{
    public string Id { get; set; }
    public string OriginalMessageId { get; set; }
    public string Payload { get; set; }
    public int FinalAttemptCount { get; set; }
    public DateTimeOffset SuccessAt { get; set; }
}