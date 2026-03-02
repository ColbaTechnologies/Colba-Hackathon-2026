namespace ActorBaseMessaging.Application.Services;

using System.Collections.Concurrent;
using System.Text.Json;
using Actors;
using Domain.Interfaces;
using DTOs;
using Interfaces;
using Microsoft.Extensions.Logging;
using Proto;

/// <summary>
/// Singleton service that owns the <see cref="ActorSystem"/> and acts as the
/// registry between HTTP request IDs and actor PIDs.
///
/// On startup <see cref="InitializeAsync"/> queries the repository for any
/// Pending or Retrying records left over from a previous process and re-spawns
/// their actors so delivery continues automatically (crash recovery).
/// </summary>
public sealed class MessageActorSystem(
    ActorSystem        system,
    IMessageRepository repository,
    IMessageForwarder  forwarder,
    ILoggerFactory     loggerFactory) : IMessageActorSystem, IDisposable
{
    private readonly ConcurrentDictionary<string, PID> _registry = new();
    private readonly ILogger _logger = loggerFactory.CreateLogger<MessageActorSystem>();

    // ── Startup ───────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Starting crash recovery: querying in-flight requests.");

        try
        {
            var inflight = await repository.GetInFlightAsync();

            _logger.LogInformation("Recovery: found {Count} in-flight record(s) to resume.", inflight.Count);

            foreach (var req in inflight)
            {
                JsonElement payloadElement;
                using (var jsonDoc = JsonDocument.Parse(req.RawPayload))
                    payloadElement = jsonDoc.RootElement.Clone();

                Enqueue(req.Id, req.TargetUrl, payloadElement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Crash recovery query failed. In-flight messages from the previous run will not be retried automatically.");
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void Enqueue(string requestId, string targetUrl, JsonElement payload)
    {
        if (_registry.ContainsKey(requestId))
            return;

        var logger = loggerFactory.CreateLogger<MessageActor>();

        var props = Props.FromProducer(() =>
            new MessageActor(requestId, targetUrl, payload, repository, forwarder, logger));

        var pid = system.Root.Spawn(props);
        _registry[requestId] = pid;
    }

    public async Task<MessageStatusDto?> GetStatusAsync(string requestId)
    {
        if (_registry.TryGetValue(requestId, out var pid))
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try
            {
                var response = await system.Root.RequestAsync<StatusResponse>(
                    pid, new GetStatus(), cts.Token);

                return new MessageStatusDto(
                    RequestId:   response.RequestId,
                    TargetUrl:   response.TargetUrl,
                    State:       response.State,
                    RetryCount:  response.RetryCount,
                    ReceivedAt:  response.ReceivedAt,
                    DeliveredAt: response.DeliveredAt);
            }
            catch (OperationCanceledException) { }
        }

        return await GetStatusFromRepositoryAsync(requestId);
    }

    public void Dispose() => system.ShutdownAsync().GetAwaiter().GetResult();

    // ── Repository fallback ───────────────────────────────────────────────────

    private async Task<MessageStatusDto?> GetStatusFromRepositoryAsync(string requestId)
    {
        var req = await repository.GetByIdAsync(requestId);

        if (req is null) return null;

        return new MessageStatusDto(
            RequestId:   req.Id,
            TargetUrl:   req.TargetUrl,
            State:       req.State,
            RetryCount:  req.RetryCount,
            ReceivedAt:  req.ReceivedAt,
            DeliveredAt: req.DeliveredAt);
    }
}
