using Producer.Models;
using Raven.Client.Documents;

namespace Producer.Worker;

public class ErrorMessageReprocessingJob(
    IDocumentStore store,
    IDocumentWorkerFactory workerFactory,
    ILogger<ErrorMessageReprocessingJob> logger)
    : BackgroundService
{
    private const int MaxRetries = 3;
    private const int ProcessingIntervalSeconds = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ErrorMessageReprocessingJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReprocessFailedMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ReprocessFailedMessages");
            }

            await Task.Delay(TimeSpan.FromSeconds(ProcessingIntervalSeconds), stoppingToken);
        }

        logger.LogInformation("ErrorMessageReprocessingJob stopped");
    }

    
    private async Task ReprocessFailedMessages(CancellationToken stoppingToken)
    {
        using var session = store.OpenAsyncSession();

        var failedMessages = await session.Query<ErrorMessage>()
            .Where(m => m.RetryCount < MaxRetries && !m.IsMarkedAsUnprocessable)
            .Take(100)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(stoppingToken);

        logger.LogInformation("{Count} error messages pending to be reprocessed", failedMessages.Count);

        if (failedMessages.Count is 0)
        {
            return;
        }

        await Parallel.ForEachAsync(failedMessages, new ParallelOptions
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = stoppingToken
        }, async (message, ct) =>
        {
            using var workerSession = store.OpenAsyncSession();
            
            try
            {
                var worker = workerFactory.Create();
                await worker.ProcessAsync(message.Id!, ct);
                
                message.RetryCount++;
                await workerSession.StoreAsync(message, ct);
                await workerSession.SaveChangesAsync(ct);
                
                logger.LogInformation("ErrorMessage {MessageId} reprocessed successfully", message.Id);
            }
            catch (Exception ex)
            {
                message.RetryCount++;

                if (message.RetryCount >= MaxRetries)
                {
                    message.IsMarkedAsUnprocessable = true;
                    logger.LogWarning(
                        ex,
                        "ErrorMessage {MessageId} marked as unprocessable after {RetryCount} retries",
                        message.Id,
                        message.RetryCount);
                }
                else
                {
                    logger.LogWarning(
                        ex,
                        "ErrorMessage {MessageId} reprocessing failed (attempt {RetryCount}/{MaxRetries})",
                        message.Id,
                        message.RetryCount,
                        MaxRetries);
                }

                await workerSession.StoreAsync(message, ct);
                await workerSession.SaveChangesAsync(ct);
            }
        });
    }
}