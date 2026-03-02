using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.CompareExchange;

namespace Equipo1_QStash_Clone.Services;

public class ChannelSeeder(QueueRepository queueRepository, IDocumentStore store) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cmpXchgResult = store.Operations.Send(new PutCompareExchangeValueOperation<string>("lock", "lock", 0));
        var session = store.OpenSession();

        var queues = session.Query<Queue>().ToList();
        foreach (var queue in queues)
        {
            queueRepository.CreateQueue(queue.Id, queue.Fifo);
          
        }
        
        var deleteResult = store.Operations.Send(new DeleteCompareExchangeValueOperation<string>("lock",0));
        
        return Task.CompletedTask;
    }
}