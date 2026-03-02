namespace Producer.Models;

public class CreateMessageRequest
{
    public string? Payload { get; set; }
    public string? Url { get; set; }
}