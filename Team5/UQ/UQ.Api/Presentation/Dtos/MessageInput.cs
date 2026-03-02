namespace UQ.Api.Presentation.Dtos;

public class MessageInput
{
    public required string DestinationUrl { get; set; }
    public required string Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTimeOffset? ScheduledOn { get; set; }
    public string? CallbackUrl { get; set; }
    public string? CallerRequestId { get; set; }
}