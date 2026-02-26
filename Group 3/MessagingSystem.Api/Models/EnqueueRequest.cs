namespace WebApplication1.Models;

public class EnqueueRequest
{
    public required string DestinationUrl { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public object? Payload { get; set; }
    
    public string? ClientMessageId { get; set; }
    public string? TenantId { get; set; }
    public string? CallbackUrl { get; set; }
    public DateTime? DeliverAtUtc { get; set; }
}