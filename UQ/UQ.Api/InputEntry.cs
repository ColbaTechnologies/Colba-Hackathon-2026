namespace UQ.Api;

public class InputEntry
{
    // TODO: add documentation for all fields
    public required string DestinationUri { get; set; }
    public required string Body { get; set; }
    public Dictionary<string, string> Headers { get; set; } = [];

    public DateTimeOffset? ScheduledOn { get; set; }
    public string? CallbackUrl { get; set; }
    public string? CallerRequestId { get; set; }
}