using System.Text.Json.Serialization;
using MessagingSystem.Domain.Enums;

namespace MessagingSystem.Application.Dtos;

public sealed class MessageSummary
{
    public string MessageId { get; init; }
    public string DestinationUrl { get; init; } = default!;
    
    public string? TopicName { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MessageStatus Status { get; init; }
    public int AttemptCount { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? LastAttemptAtUtc { get; init; }
    public DateTimeOffset? NextAttemptAtUtc { get; init; }
    public string? LastError { get; init; }
    public string? TenantId { get; init; }
}