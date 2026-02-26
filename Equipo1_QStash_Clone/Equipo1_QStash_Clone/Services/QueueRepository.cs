using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;

namespace Equipo1_QStash_Clone.Services;

public class QueueRepository(ILogger<QueueRepository> logger, IDocumentStore store)
{
    private static Dictionary<string, Channel<string>>  _queue = new();
    private static Dictionary<string, Consumer >  _consummer = new();

    public Channel<string> GetChannelQueue(string queueId)
    {
        return _queue[queueId];
    }

    public void CreateQueue(string queueId)
    {
        _queue[queueId] = Channel.CreateUnbounded<string>();
        var consumer = new Consumer(logger, _queue[queueId], store);
        _ = Task.Run(async () => {await consumer.Start(queueId); });
        _consummer.Add(queueId, consumer);
        
        var session = store.OpenSession();   
        var messages = session.Query<PersistedMessage>().Where(x=> x.QueueId == queueId).ToList();
          
        foreach (var message in messages)
        {
            _queue[queueId].Writer.TryWrite(message.Id); 
            logger.LogInformation("Publishing pending messages to queue {queueId}", queueId);
        }
        logger.LogInformation("Queue {QueueId} created", queueId);
    }

    public void DeleteQueue(string queueId)
    {
        _consummer[queueId].Stop();
        _consummer.Remove(queueId);
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