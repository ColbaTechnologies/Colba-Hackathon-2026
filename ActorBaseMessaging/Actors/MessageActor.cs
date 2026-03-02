namespace ActorBaseMessaging.Actors;

using System.Text;
using System.Text.Json;
using Models;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Timers;
using Raven.Client.Documents;

/// <summary>
/// One actor instance = one outbound request.
///
/// Lifecycle:
///   Spawned → Started → load RavenDB doc
///     ├─ doc found   → restore state (crash recovery)
///     └─ doc missing → persist Pending, then self-send ProcessRequest
///
///   ProcessRequest / RetryRequest
///     → HTTP POST → mutate state → persist state to RavenDB
///       ✓ Delivered  → actor idles (queryable via GetStatus)
///       ✗ Retrying   → scheduler sends RetryRequest after back-off delay
///       ✗ Erroneous  → actor idles after max retries exhausted
///
/// All async operations use ReenterAfter so state mutations always happen
/// inside the actor's single-threaded execution context — no locks needed.
/// </summary>
public sealed class MessageActor(
    string             requestId,
    string             targetUrl,
    JsonElement        payload,
    IHttpClientFactory httpClientFactory,
    ILogger            logger,
    IDocumentStore     documentStore) : IActor
{
    private const int MaxRetries = 3;

    private static string DocId(string id) => $"messages/{id}";

    // Transformed once at construction — all other ctor params are captured directly.
    private readonly string _rawPayload = payload.GetRawText();

    // ── Mutable actor state ───────────────────────────────────────────────────
    private MessageState _state      = MessageState.Pending;
    private int          _retryCount = 0;
    private DateTime     _receivedAt;
    private DateTime?    _deliveredAt;

    private CancellationTokenSource? _retrySchedule;

    // ── Proto.Actor message pump ──────────────────────────────────────────────

    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                _receivedAt = DateTime.UtcNow;
                LoadStateAndInitialize(context);
                break;

            case ProcessRequest:
            case RetryRequest:
                AttemptDelivery(context);
                break;

            case GetStatus:
                context.Respond(BuildSnapshot());
                break;

            case Stopping:
                _retrySchedule?.Cancel();
                break;
        }

        return Task.CompletedTask;
    }

    // ── Startup / Recovery ────────────────────────────────────────────────────

    /// <summary>
    /// On actor start:
    ///   1. Load existing document from RavenDB (ReenterAfter — outside actor context).
    ///   2a. Document found  → restore in-memory state; re-trigger delivery if needed.
    ///   2b. Document missing → persist initial Pending doc, then begin delivery.
    /// </summary>
    private void LoadStateAndInitialize(IContext context)
    {
        context.ReenterAfter(LoadDocumentAsync(), async loadTask =>
        {
            var doc = await loadTask;

            if (doc is not null)
            {
                // ── Recovery path ────────────────────────────────────────────
                _state       = doc.State;
                _retryCount  = doc.RetryCount;
                _receivedAt  = doc.ReceivedAt;
                _deliveredAt = doc.DeliveredAt;

                logger.LogInformation(
                    "[{Id}] Recovered from RavenDB. State={State}, RetryCount={Retries}.",
                    requestId, _state, _retryCount);

                if (_state is MessageState.Pending or MessageState.Retrying)
                    context.Send(context.Self, new ProcessRequest());
            }
            else
            {
                // ── New-request path ─────────────────────────────────────────
                // Persist initial Pending state first so the document exists
                // before the first delivery attempt is recorded.
                context.ReenterAfter(SaveDocumentAsync(), async saveTask =>
                {
                    await saveTask;

                    logger.LogInformation(
                        "[{Id}] Initial state persisted. Scheduling delivery to {Url}.",
                        requestId, targetUrl);

                    context.Send(context.Self, new ProcessRequest());
                });
            }
        });
    }

    // ── Delivery logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Fires the HTTP POST, mutates state in the actor context via ReenterAfter,
    /// then persists the new state. The retry schedule is only set after the
    /// state has been durably written.
    /// </summary>
    private void AttemptDelivery(IContext context)
    {
        logger.LogInformation("[{Id}] Attempting delivery (attempt {Attempt}/{Max}).",
            requestId, _retryCount + 1, MaxRetries + 1);

        var client   = httpClientFactory.CreateClient("MessageForwarder");
        var content  = new StringContent(_rawPayload, Encoding.UTF8, "application/json");
        var httpTask = client.PostAsync(targetUrl, content);

        context.ReenterAfter(httpTask, async t =>
        {
            try
            {
                var response = await t;
                response.EnsureSuccessStatusCode();

                _state       = MessageState.Delivered;
                _deliveredAt = DateTime.UtcNow;

                logger.LogInformation("[{Id}] Delivered successfully at {At}.",
                    requestId, _deliveredAt);
            }
            catch (Exception ex)
            {
                if (_retryCount >= MaxRetries)
                {
                    _state = MessageState.Erroneous;
                    logger.LogError(ex,
                        "[{Id}] All {Max} retries exhausted. Marking as Erroneous.",
                        requestId, MaxRetries);
                }
                else
                {
                    _retryCount++;
                    _state = MessageState.Retrying;

                    logger.LogWarning(ex,
                        "[{Id}] Delivery failed. Retry {Attempt}/{Max} scheduled.",
                        requestId, _retryCount, MaxRetries);
                }
            }

            // Persist the new state, then (if Retrying) arm the scheduler.
            // The retry is scheduled only after the state is durably written
            // to avoid a gap where a restart could skip the retry.
            context.ReenterAfter(SaveDocumentAsync(), async saveTask =>
            {
                await saveTask;

                if (_state == MessageState.Retrying)
                {
                    var delay = RetryDelay(_retryCount);
                    _retrySchedule?.Cancel();
                    _retrySchedule = context.Scheduler()
                        .SendOnce(delay, context.Self, new RetryRequest());
                }
            });
        });
    }

    // ── RavenDB helpers ───────────────────────────────────────────────────────

    private async Task<MessageDocument?> LoadDocumentAsync()
    {
        using var session = documentStore.OpenAsyncSession();
        return await session.LoadAsync<MessageDocument>(DocId(requestId));
    }

    private async Task SaveDocumentAsync()
    {
        using var session = documentStore.OpenAsyncSession();
        await session.StoreAsync(BuildDocument(), DocId(requestId));
        await session.SaveChangesAsync();
    }

    private MessageDocument BuildDocument() => new()
    {
        Id          = DocId(requestId),
        RequestId   = requestId,
        TargetUrl   = targetUrl,
        RawPayload  = _rawPayload,
        State       = _state,
        RetryCount  = _retryCount,
        ReceivedAt  = _receivedAt,
        DeliveredAt = _deliveredAt,
    };

    // ── Snapshot / back-off ───────────────────────────────────────────────────

    private StatusResponse BuildSnapshot() =>
        new(
            RequestId:   requestId,
            TargetUrl:   targetUrl,
            State:       _state,
            RetryCount:  _retryCount,
            ReceivedAt:  _receivedAt,
            DeliveredAt: _deliveredAt
        );

    private static TimeSpan RetryDelay(int attempt) =>
        attempt switch
        {
            1 => TimeSpan.FromSeconds(5),
            2 => TimeSpan.FromSeconds(30),
            _ => TimeSpan.FromMinutes(2),
        };
}
