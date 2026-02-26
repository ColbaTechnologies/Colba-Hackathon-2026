namespace ActorBaseMessaging.Services;

using System.Collections.Concurrent;
using System.Text.Json;
using Actors;
using Models;
using Microsoft.Extensions.Logging;
using Proto;

/// <summary>
/// Singleton service that owns the <see cref="ActorSystem"/> and acts as the
/// registry between HTTP request IDs and actor PIDs.
/// </summary>
public sealed class MessageActorSystem(
    ActorSystem        system,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory     loggerFactory) : IDisposable
{
    // requestId → PID of the per-request actor
    private readonly ConcurrentDictionary<string, PID> _registry = new();

    /// <summary>
    /// Spawns a new <see cref="MessageActor"/> for the given request and returns
    /// the generated request ID that the caller can use to poll status.
    /// </summary>
    public string Enqueue(string targetUrl, JsonElement payload)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var logger    = loggerFactory.CreateLogger<MessageActor>();

        var props = Props.FromProducer(() =>
            new MessageActor(requestId, targetUrl, payload, httpClientFactory, logger));

        var pid = system.Root.Spawn(props);
        _registry[requestId] = pid;

        return requestId;
    }

    /// <summary>
    /// Queries the actor for its current status snapshot.
    /// Returns <c>null</c> if the request ID is unknown.
    /// </summary>
    public async Task<StatusResponse?> GetStatusAsync(string requestId)
    {
        if (!_registry.TryGetValue(requestId, out var pid))
            return null;

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            return await system.Root.RequestAsync<StatusResponse>(
                pid, new GetStatus(), cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Actor may have stopped; return null and let the caller decide
            return null;
        }
    }

    public void Dispose() => system.ShutdownAsync().GetAwaiter().GetResult();
}
