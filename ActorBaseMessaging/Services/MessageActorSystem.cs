namespace ActorBaseMessaging.Services;

using System.Collections.Concurrent;
using System.Text.Json;
using Actors;
using Models;
using Microsoft.Extensions.Logging;
using Proto;
using Raven.Client.Documents;

/// <summary>
/// Singleton service that owns the <see cref="ActorSystem"/> and acts as the
/// registry between HTTP request IDs and actor PIDs.
///
/// On startup <see cref="InitializeAsync"/> queries RavenDB for any Pending or
/// Retrying documents left over from a previous process and re-spawns their
/// actors so delivery continues automatically (crash recovery).
/// </summary>
public sealed class MessageActorSystem(
    ActorSystem        system,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory     loggerFactory,
    IDocumentStore     documentStore) : IDisposable
{
    private readonly ConcurrentDictionary<string, PID> _registry = new();
    private readonly ILogger _logger = loggerFactory.CreateLogger<MessageActorSystem>();

    // ── Startup ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Called once from Program.cs before the HTTP server starts serving requests.
    /// Re-enqueues any in-flight messages (Pending / Retrying) found in RavenDB.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting crash recovery: querying in-flight requests from RavenDB.");

        try
        {
            List<MessageDocument> inflight;

            using (var session = documentStore.OpenAsyncSession())
            {
                inflight = await session.Query<MessageDocument>()
                    .Where(d => d.State == MessageState.Pending || d.State == MessageState.Retrying)
                    .ToListAsync();
            }

            _logger.LogInformation("Recovery: found {Count} in-flight document(s) to resume.", inflight.Count);

            foreach (var doc in inflight)
            {
                // JsonDocument is IDisposable; Clone() gives a self-contained
                // JsonElement that survives the using block.
                JsonElement payloadElement;
                using (var jsonDoc = JsonDocument.Parse(doc.RawPayload))
                    payloadElement = jsonDoc.RootElement.Clone();

                Enqueue(doc.RequestId, doc.TargetUrl, payloadElement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Crash recovery query failed. In-flight messages from the previous run will not be retried automatically.");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns a new <see cref="MessageActor"/> for the given request.
    /// Idempotent: if the requestId is already registered the call is a no-op.
    /// </summary>
    public void Enqueue(string requestId, string targetUrl, JsonElement payload)
    {
        if (_registry.ContainsKey(requestId))
            return;

        var logger = loggerFactory.CreateLogger<MessageActor>();

        var props = Props.FromProducer(() =>
            new MessageActor(requestId, targetUrl, payload, httpClientFactory, logger, documentStore));

        var pid = system.Root.Spawn(props);
        _registry[requestId] = pid;
    }

    /// <summary>
    /// Queries the live actor for its current status snapshot.
    /// Falls back to reading directly from RavenDB when the PID is not in the
    /// in-memory registry (e.g. after a restart for a Delivered / Erroneous request
    /// that was not recovered because it needed no further action).
    /// </summary>
    public async Task<StatusResponse?> GetStatusAsync(string requestId)
    {
        if (_registry.TryGetValue(requestId, out var pid))
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                return await system.Root.RequestAsync<StatusResponse>(
                    pid, new GetStatus(), cts.Token);
            }
            catch (OperationCanceledException) { }
        }

        return await GetStatusFromDbAsync(requestId);
    }

    public void Dispose() => system.ShutdownAsync().GetAwaiter().GetResult();

    // ── DB fallback ───────────────────────────────────────────────────────────

    private async Task<StatusResponse?> GetStatusFromDbAsync(string requestId)
    {
        using var session = documentStore.OpenAsyncSession();
        var doc = await session.LoadAsync<MessageDocument>($"messages/{requestId}");

        if (doc is null) return null;

        return new StatusResponse(
            RequestId:   doc.RequestId,
            TargetUrl:   doc.TargetUrl,
            State:       doc.State,
            RetryCount:  doc.RetryCount,
            ReceivedAt:  doc.ReceivedAt,
            DeliveredAt: doc.DeliveredAt);
    }
}
