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
    IProcessorIdentifier processorIdentifier,
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
                        TimeSpan.FromMilliseconds(20),
                        stoppingToken);

                    continue;
                }
                
                var tasks = dueMessages.Select(async message =>
                {
                    var claimed = await store.TryClaimMessageForProcessingAsync(
                        message.Id,
                        processorIdentifier.InstanceId,
                        stoppingToken);

                    if (claimed != null)
                    {
                        try
                        {
                            var result = await processor.ProcessAsync(message, stoppingToken);

                            if (result.Status == MessageStatus.Retry)
                            {
                                await store.MoveToRetryCollectionAsync(message, stoppingToken);
                            }
                            else
                            {
                                await store.MarkAsCompletedAsync(message, stoppingToken);
                                await callbackNotifier.NotifyAsync(
                                    message,
                                    "Delivered",
                                    stoppingToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(
                                ex,
                                "Processing failed for message {MessageId}",
                                message.Id);
                        }
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
                    TimeSpan.FromMilliseconds(20),
                    stoppingToken);
            }
        }

        logger.LogInformation("Message Dispatcher stopped.");
    }
}