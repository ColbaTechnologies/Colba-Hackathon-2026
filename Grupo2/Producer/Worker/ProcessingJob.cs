using Producer.Models;
using Raven.Client.Documents;

namespace Producer.Worker;

public class ProcessingJob(IDocumentStore store, IDocumentWorkerFactory workerFactory, ILogger<ProcessingJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ProcessingJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessages(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ProcessPendingMessages");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        logger.LogInformation("ProcessingJob stopped");
    }

    private async Task ProcessPendingMessages(CancellationToken stoppingToken)
    {
        using var session = store.OpenAsyncSession();

        var pendingMessages = await session.Query<ReceivedMessage>()
            .Take(100)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(stoppingToken);

        logger.LogInformation("{Count} received messages pending to be processed", pendingMessages.Count);

        if (pendingMessages.Count is 0)
        {
            return;
        }

        await Parallel.ForEachAsync(pendingMessages, new ParallelOptions
        {
            MaxDegreeOfParallelism = 10,
            CancellationToken = stoppingToken
        }, async (message, ct) =>
        {
            var worker = workerFactory.Create();
            await worker.ProcessAsync(message.Id!, ct);
        });
    }
}