namespace ActorBaseMessaging.Application.DTOs;

using Domain.Enums;

public record MessageStatusDto(
    string       RequestId,
    string       TargetUrl,
    MessageState State,
    int          RetryCount,
    DateTime     ReceivedAt,
    DateTime?    DeliveredAt
);
