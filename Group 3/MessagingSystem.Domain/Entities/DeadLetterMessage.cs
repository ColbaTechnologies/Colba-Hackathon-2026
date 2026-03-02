namespace MessagingSystem.Domain.Entities;

public class DeadLetterMessage
{
    public string Id { get; set; }
    public string OriginalMessageId { get; set; }
    public string Payload { get; set; }
    public int FinalAttemptCount { get; set; }
    public string LastError { get; set; }
    public DateTimeOffset FailedAt { get; set; }
}