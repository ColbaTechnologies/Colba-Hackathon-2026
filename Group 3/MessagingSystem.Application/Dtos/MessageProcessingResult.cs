using MessagingSystem.Domain.Enums;

namespace MessagingSystem.Application.Dtos;

public sealed record MessageProcessingResult(
    MessageStatus Status,
    int AttemptCount,
    DateTimeOffset LastAttemptAtUtc,
    DateTimeOffset? NextAttemptAtUtc,
    string? LastError);