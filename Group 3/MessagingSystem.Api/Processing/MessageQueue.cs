namespace WebApplication1.Processing;

public class MessageQueue : IMessageQueue
{
    public void Enqueue(Guid messageId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}