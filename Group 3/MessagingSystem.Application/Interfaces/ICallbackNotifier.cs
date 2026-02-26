using MessagingSystem.Domain.Entities;

namespace MessagingSystem.Application.Interfaces;

public interface ICallbackNotifier
{
    Task NotifyAsync(
        ReceivedMessage message,
        string status,
        CancellationToken ct);
}