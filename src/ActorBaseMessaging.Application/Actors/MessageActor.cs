namespace ActorBaseMessaging.Application.Actors;

using System.Text.Json;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Timers;

/// <summary>
/// One actor instance = one outbound request.
///
/// Lifecycle:
///   Spawned → Started → load persisted MessageRequest
///     ├─ doc found   → restore state (crash recovery)
///     └─ doc missing → persist Pending, then self-send ProcessRequest
///
///   ProcessRequest / RetryRequest
///     → HTTP POST → mutate entity state → persist
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
    IMessageRepository repository,
    IMessageForwarder  forwarder,
    ILogger            logger) : IActor
{
    private const int MaxRetries = 3;

    private readonly string _rawPayload = payload.GetRawText();

    private MessageRequest? _request;
    private CancellationTokenSource? _retrySchedule;

    // ── Proto.Actor message pump ──────────────────────────────────────────────

    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
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

    private void LoadStateAndInitialize(IContext context)
    {
        context.ReenterAfter(repository.GetByIdAsync(requestId), async loadTask =>
        {
            var existing = await loadTask;

            if (existing is not null)
            {
                _request = existing;

                logger.LogInformation(
                    "[{Id}] Recovered. State={State}, RetryCount={Retries}.",
                    requestId, _request.State, _request.RetryCount);

                if (_request.IsInProgress)
                    context.Send(context.Self, new ProcessRequest());
            }
            else
            {
                _request = MessageRequest.Create(requestId, targetUrl, _rawPayload);

                context.ReenterAfter(repository.SaveAsync(_request), async saveTask =>
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

    private void AttemptDelivery(IContext context)
    {
        logger.LogInformation("[{Id}] Attempting delivery (attempt {Attempt}/{Max}).",
            requestId, (_request?.RetryCount ?? 0) + 1, MaxRetries + 1);

        var forwardTask = forwarder.ForwardAsync(targetUrl, _rawPayload);

        context.ReenterAfter(forwardTask, async t =>
        {
            try
            {
                await t;
                _request!.MarkDelivered(DateTime.UtcNow);

                logger.LogInformation("[{Id}] Delivered successfully at {At}.",
                    requestId, _request.DeliveredAt);
            }
            catch (Exception ex)
            {
                if (!_request!.CanRetry(MaxRetries))
                {
                    _request.MarkErroneous();
                    logger.LogError(ex,
                        "[{Id}] All {Max} retries exhausted. Marking as Erroneous.",
                        requestId, MaxRetries);
                }
                else
                {
                    _request.MarkRetrying();
                    logger.LogWarning(ex,
                        "[{Id}] Delivery failed. Retry {Attempt}/{Max} scheduled.",
                        requestId, _request.RetryCount, MaxRetries);
                }
            }

            context.ReenterAfter(repository.SaveAsync(_request!), async saveTask =>
            {
                await saveTask;

                if (_request!.State == Domain.Enums.MessageState.Retrying)
                {
                    var delay = RetryDelay(_request.RetryCount);
                    _retrySchedule?.Cancel();
                    _retrySchedule = context.Scheduler()
                        .SendOnce(delay, context.Self, new RetryRequest());
                }
            });
        });
    }

    // ── Snapshot / back-off ───────────────────────────────────────────────────

    private StatusResponse BuildSnapshot()
    {
        if (_request is null)
            return new StatusResponse(requestId, targetUrl, Domain.Enums.MessageState.Pending, 0, DateTime.UtcNow, null);

        return new StatusResponse(
            RequestId:   _request.Id,
            TargetUrl:   _request.TargetUrl,
            State:       _request.State,
            RetryCount:  _request.RetryCount,
            ReceivedAt:  _request.ReceivedAt,
            DeliveredAt: _request.DeliveredAt
        );
    }

    private static TimeSpan RetryDelay(int attempt) =>
        attempt switch
        {
            1 => TimeSpan.FromSeconds(5),
            2 => TimeSpan.FromSeconds(30),
            _ => TimeSpan.FromMinutes(2),
        };
}
