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
    IProcessorIdentifier processorIdentifier,
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
                        TimeSpan.FromMilliseconds(30),
                        stoppingToken);

                    continue;
                }

                // TODO use the procesorIdentifier y el tryget
                
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
                        
                        await store.UpdateRetries(message.Id, message.AttemptCount, stoppingToken); 
                        continue;
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
                    TimeSpan.FromMilliseconds(20),
                    stoppingToken);
            }
        }

        logger.LogInformation("Retry Dispatcher stopped.");
    }
}