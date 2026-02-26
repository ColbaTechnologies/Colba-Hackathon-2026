using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Raven.Client.Documents;

namespace Equipo1_QStash_Clone.Services;

public class ChannelSeeder(QueueRepository queueRepository, IDocumentStore store) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var session = store.OpenSession();

        var queues = session.Query<Queue>().ToList();
        foreach (var queue in queues)
        {
            queueRepository.CreateQueue(queue.Id);
          
        }
        
        return Task.CompletedTask;
    }
}