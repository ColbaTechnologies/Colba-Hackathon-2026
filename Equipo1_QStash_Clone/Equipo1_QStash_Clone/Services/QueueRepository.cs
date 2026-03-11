using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Equipo1_QStash_Clone.Services;

// TODO @HACKATHON - why not async? - use concurrent dictionaries?
// TODO @HACKATHON - not scallable, should separate creating the queue from adding the first messages, state is keep locally, if a instance without the queue is called it will return error
// TODO @HACKATHON - on start all instances should load the queues but only one instance will start processing pending messages
public class QueueRepository(ILogger<QueueRepository> logger, IDocumentStore store, QueueMetrics metrics)
{
    private static readonly Dictionary<string, Channel<string>>  Queue = new();
    private static readonly Dictionary<string, Consumer >  Consumer = new();

    public Channel<string> GetChannelQueue(string queueId)
    {
        return Queue[queueId];
    }

    public void CreateQueue(string queueId, bool fifo = true)
    {
        Queue[queueId] = Channel.CreateUnbounded<string>();
        var consumer = new Consumer(logger, Queue[queueId], store, metrics, fifo);
        _ = Task.Run(async () => { await consumer.Start(queueId); });
        Consumer.Add(queueId, consumer);
        
        // TODO @HACKATHON - why not async?
        var session = store.OpenSession();
        var messages = session.
            Query<PersistedMessage>()
            .Where(x=> x.QueueId == queueId)
            .OrderByDescending(x=> x.Timestamp)
            .ToList();
          
        foreach (var message in messages)
        {
            Queue[queueId].Writer.TryWrite(message.Id); 
            logger.LogInformation("Publishing pending messages to queue {queueId}", queueId);
        }
        logger.LogInformation("Queue {QueueId} created", queueId);
    }

    public void DeleteQueue(string queueId)
    {
        Consumer[queueId].Stop();
        Consumer.Remove(queueId);
    }
    
    public int DeleteMessages(string queueId)
    {
        var session = store.OpenSession();   
        var messages = session.Query<PersistedMessage>().Where(x=> x.QueueId == queueId).ToList();
          
         session.Delete(messages);
        
        logger.LogInformation("Empty message {messages} Queue {QueueId} ", messages.Count, queueId);
        
        return messages.Count;
    }
}