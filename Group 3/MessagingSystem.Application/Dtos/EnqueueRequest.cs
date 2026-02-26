namespace MessagingSystem.Application.Dtos;

public class EnqueueRequest
{
    public string DestinationUrl { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public object? Payload { get; set; }
    public string? TenantId { get; set; }
    public string? CallbackUrl { get; set; }
}