namespace UQ.Api.Domain;

public class Message : IAuditable
{
    public Message(ExistingMessageInput input)
    {
        Id = input.Id;
        PublicId = input.PublicId;
        DestinationUrl = input.DestinationUrl;
        Headers = input.Headers;
        Body = input.Body;
        State = input.State;

        ScheduledOn = input.ScheduledOn;
        CallbackUrl = input.CallbackUrl;
        CallerRequestId = input.CallerRequestId;

        CreatedAt = input.CreatedAt;
        UpdatedAt = input.UpdatedAt;
    }

    public Message(CreateMessageInput input)
    {
        Id = Guid.NewGuid().ToString();
        PublicId = Guid.NewGuid().ToString();
        DestinationUrl = input.DestinationUrl;
        Headers = input.Headers;
        Body = input.Body;
        State = MessageState.Pending;

        ScheduledOn = input.ScheduledOn;
        CallbackUrl = input.CallbackUrl;
        CallerRequestId = input.CallerRequestId;

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string Id { get; set; }
    public string PublicId { get; set; }
    public string DestinationUrl { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];
    public string Body { get; set; }
    public MessageState State { get; set; }


    public DateTimeOffset? ScheduledOn { get; set; }
    public string? CallbackUrl { get; set; }
    public string? CallerRequestId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public record CreateMessageInput(
    string DestinationUrl,
    Dictionary<string, string> Headers,
    string Body,
    DateTimeOffset? ScheduledOn = null,
    string? CallbackUrl = null,
    string? CallerRequestId = null);

public record ExistingMessageInput(
    string Id,
    string PublicId,
    string DestinationUrl,
    Dictionary<string, string> Headers,
    string Body,
    MessageState State,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ScheduledOn = null,
    string? CallbackUrl = null,
    string? CallerRequestId = null);