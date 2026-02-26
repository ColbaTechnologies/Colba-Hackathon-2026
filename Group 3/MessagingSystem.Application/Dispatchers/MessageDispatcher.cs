using MessagingSystem.Application.Configuration;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MessagingSystem.Application.Dispatchers;

public sealed class MessageDispatcher(
    ILogger<MessageDispatcher> logger,
    IMessageStore store,
    IMessageProcessor processor,
    ICallbackNotifier callbackNotifier,
    IOptions<RetrySettings> settings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation("Message Dispatcher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;

                var dueMessages =
                    await store.GetPendingMessagesAsync(
                        now,
                        stoppingToken);

                if (dueMessages.Count == 0)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        stoppingToken);

                    continue;
                }

                // Backpressure - explicit demo - Modern: await Parallel.ForEachAsync
                using var semaphore = new SemaphoreSlim(settings.Value.MaxParallelism);
                var tasks = dueMessages.Select(async message =>
                {
                    await semaphore.WaitAsync(stoppingToken);

                    try
                    {
                        var result =
                            await processor.ProcessAsync(
                                message,
                                stoppingToken);

                        if (result.Status == MessageStatus.Retry)
                        {
                            message.AttemptCount++;
                            await store.MoveToRetryCollectionAsync(
                                message,
                                stoppingToken);
                        }
                        else
                        {
                            await store.MarkAsCompletedAsync(
                                message,
                                stoppingToken);
                            
                            await callbackNotifier.NotifyAsync(message, "Delivered", stoppingToken);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
                
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unexpected error during Message Dispatcher.");

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    stoppingToken);
            }
        }

        logger.LogInformation("Message Dispatcher stopped.");
    }
}