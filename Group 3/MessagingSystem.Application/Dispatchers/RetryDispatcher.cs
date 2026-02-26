using MessagingSystem.Application.Configuration;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Domain.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagingSystem.Application.Dispatchers;

public sealed class RetryDispatcher(
    ILogger<RetryDispatcher> logger,
    IMessageStore store,
    IMessageProcessor processor,
    RetrySettings settings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        logger.LogInformation("Retry Dispatcher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dueMessages =
                    await store.GetDueRetriesAsync(
                        DateTimeOffset.UtcNow,
                        stoppingToken);

                if (dueMessages.Count == 0)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(3),
                        stoppingToken);

                    continue;
                }

                foreach (var message in dueMessages)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    var result =
                        await processor.ProcessAsync(
                            message.OriginalMessage,
                            stoppingToken);

                    if (result.Status is MessageStatus.Failed or MessageStatus.Retry)
                    {
                        message.AttemptCount++;

                        if (message.AttemptCount >= settings.MaxAttempts)
                        {
                            await store.MoveToDeadLetterAsync(
                                message,
                                result,
                                stoppingToken);

                            continue;
                        }
                    }

                    await store.MarkAsCompletedAsync(message.OriginalMessage, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Retry dispatcher error.");
                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    stoppingToken);
            }
        }

        logger.LogInformation("Retry Dispatcher stopped.");
    }
}