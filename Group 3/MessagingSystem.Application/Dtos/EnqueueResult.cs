namespace MessagingSystem.Application.Dtos;

public sealed class EnqueueResult
{
    public string Id { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public string? TenantId { get; set; }
}