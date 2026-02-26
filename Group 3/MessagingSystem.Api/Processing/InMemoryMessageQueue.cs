using System.Collections.Concurrent;

namespace WebApplication1.Processing;

public sealed class InMemoryMessageQueue : IMessageQueue
{
    private readonly ConcurrentQueue<Guid> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Enqueue(Guid messageId)
    {
        _queue.Enqueue(messageId);
        _signal.Release();
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (_queue.TryDequeue(out var id))
            {
                return id;
            }
        }
    }
}