using Equipo1_QStash_Clone.Model;
using Raven.Client.Documents;

namespace Equipo1_QStash_Clone.Services;

// TODO @HACKATHON - no in use
public class QueueLoader (IDocumentStore store, QueueRepository queueRepository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var session = store.OpenAsyncSession();
        var queue = await session.Query<Queue>().ToListAsync(stoppingToken);
        foreach (var queueItem in queue)
        {
            queueRepository.CreateQueue(queueItem.Id);
        }
    }
}