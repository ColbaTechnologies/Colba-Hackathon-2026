namespace ActorBaseMessaging.Actors;

using System.Text;
using System.Text.Json;
using Models;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Timers;

/// <summary>
/// One actor instance = one outbound request.
///
/// Lifecycle:
///   Spawned → Started → ProcessRequest (self-send)
///     → HTTP POST to TargetUrl
///       ✓ success  → state = Delivered, actor idles (queryable forever)
///       ✗ failure  → RetryCount < MaxRetries → state = Retrying
///                                              → scheduler sends RetryRequest after delay
///                  → RetryCount >= MaxRetries → state = Erroneous, actor idles
///
/// State fields kept as plain fields – no locking needed (actors are single-threaded).
/// </summary>
public sealed class MessageActor(
    string             requestId,
    string             targetUrl,
    JsonElement        payload,
    IHttpClientFactory httpClientFactory,
    ILogger            logger) : IActor
{
    private const int MaxRetries = 3;

    // Transformed once at construction; all other ctor params are used directly.
    private readonly string _rawPayload = payload.GetRawText();

    // ── Mutable actor state ───────────────────────────────────────────────────
    private MessageState _state      = MessageState.Pending;
    private int          _retryCount = 0;
    private DateTime     _receivedAt;
    private DateTime?    _deliveredAt;

    // Kept so a running retry schedule can be cancelled if needed (e.g. manual stop)
    private CancellationTokenSource? _retrySchedule;

    // ── Proto.Actor message pump ──────────────────────────────────────────────

    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
                _receivedAt = DateTime.UtcNow;
                logger.LogInformation("[{Id}] Actor started. Scheduling first delivery to {Url}.",
                    requestId, targetUrl);
                context.Send(context.Self, new ProcessRequest());
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

    // ── Delivery logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Fires the HTTP POST and re-enters the actor context on completion so that
    /// state mutations happen inside the single-threaded actor context (no races).
    /// </summary>
    private void AttemptDelivery(IContext context)
    {
        logger.LogInformation("[{Id}] Attempting delivery (attempt {Attempt}/{Max}).",
            requestId, _retryCount + 1, MaxRetries + 1);

        var client  = httpClientFactory.CreateClient("MessageForwarder");
        var content = new StringContent(_rawPayload, Encoding.UTF8, "application/json");
        var httpTask = client.PostAsync(targetUrl, content);

        // ReenterAfter ensures the continuation runs back inside the actor's
        // single-threaded execution context, keeping state mutations race-free.
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

                    var delay = RetryDelay(_retryCount);
                    logger.LogWarning(ex,
                        "[{Id}] Delivery failed. Retry {Attempt}/{Max} in {Delay}.",
                        requestId, _retryCount, MaxRetries, delay);

                    // Cancel any previous schedule and create a new one
                    _retrySchedule?.Cancel();
                    _retrySchedule = context.Scheduler()
                        .SendOnce(delay, context.Self, new RetryRequest());
                }
            }
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private StatusResponse BuildSnapshot() =>
        new(
            RequestId:   requestId,
            TargetUrl:   targetUrl,
            State:       _state,
            RetryCount:  _retryCount,
            ReceivedAt:  _receivedAt,
            DeliveredAt: _deliveredAt
        );

    /// <summary>Exponential-ish back-off: 5 s → 30 s → 2 min.</summary>
    private static TimeSpan RetryDelay(int attempt) =>
        attempt switch
        {
            1 => TimeSpan.FromSeconds(5),
            2 => TimeSpan.FromSeconds(30),
            _ => TimeSpan.FromMinutes(2),
        };
}
