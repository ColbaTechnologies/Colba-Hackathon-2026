namespace WebApplication1.Processing;

public interface IMessageQueue
{
    void Enqueue(Guid messageId);
    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}