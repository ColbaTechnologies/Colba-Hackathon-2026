using MessagingSystem.Application.Dtos;
using MessagingSystem.Application.Interfaces;

namespace MessagingSystem.Application.Services;

using System.Threading;

public sealed class MetricsPublisher : IMetricsPublisher
{
    private int _processed;
    private int _failed;

    public void IncrementProcessed()
        => Interlocked.Increment(ref _processed);

    public void IncrementFailed()
        => Interlocked.Increment(ref _failed);

    public MetricsSnapshot GetSnapshot()
        => new()
        {
            Processed = _processed,
            Failed = _failed
        };
}