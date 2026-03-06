namespace ActorBaseMessaging.Domain.Interfaces;

using Entities;

public interface IMessageRepository
{
    Task<MessageRequest?> GetByIdAsync(string id);
    Task SaveAsync(MessageRequest message);
    Task<IReadOnlyList<MessageRequest>> GetInFlightAsync();
    Task MarkRecoveryCompleteAsync();
    Task<DateTime?> GetRecoveryCompletedAtAsync();
}
