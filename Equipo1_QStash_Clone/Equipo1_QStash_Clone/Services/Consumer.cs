using System.Diagnostics;
using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Polly;
using Raven.Client.Documents;

namespace Equipo1_QStash_Clone.Services;

public class Consumer(ILogger logger, Channel<string> channel, IDocumentStore store, QueueMetrics metrics)
{

    private readonly HttpClient _httpClient = new();
    private bool _kill;
    
    public async Task Start(string queueId)
    {
        logger.LogInformation("Consumer started for {queueId}", queueId);
        
        while (await channel.Reader.WaitToReadAsync() && !_kill)
        {
            while(channel.Reader.TryRead(out var messageId) && !_kill)
            {
                using var session = store.OpenAsyncSession();
                try
                {
                    var message = await session.LoadAsync<PersistedMessage>(messageId);
                    
                    logger.LogInformation("Received message to {queueId}: {MessageId}",queueId, message.Id);
                    
                    var retryPolicy =
                        Policy<HttpResponseMessage>
                            .Handle<HttpRequestException>()
                            .OrResult(r => !r.IsSuccessStatusCode)
                            .WaitAndRetryAsync(
                                message.InputMessage.Retries,
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            );
                    
                    logger.LogInformation("Send message: {MessageId} {Retry}", message.Id, message.InputMessage.Retries);

                    var sw = Stopwatch.StartNew();
                    var response = await retryPolicy.ExecuteAsync(() =>
                        _httpClient.SendAsync(CreateHttpRequestMessage(message))
                    );
                    metrics.RecordDeliveryDuration(sw, queueId);

                    if (response.IsSuccessStatusCode)
                    {
                        metrics.MessageDelivered(queueId);
                    }
                    else
                    {
                        metrics.MessageDeliveryFailed(queueId);

                        await session.StoreAsync(new ErrorMessage
                        {
                            Id = Guid.NewGuid().ToString(),
                            Error = response.ReasonPhrase,
                            PersistedMessage = message
                        });

                        metrics.DeadLetterMessage(queueId);
                    }

                    session.Delete(message);
                }
                catch (Exception e)
                {
                    //TODO Save message on unmanange exception 
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    await session.SaveChangesAsync();
                }
            }
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(PersistedMessage message)
    {
        var request = new HttpRequestMessage(new HttpMethod(message.InputMessage.Method),
            message.InputMessage.Url);
        
        if (message.InputMessage.Headers == null) 
            return request;
        foreach (var header in message.InputMessage.Headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return request;
    }
    
    public void Stop()
    {
        _kill = true;
        logger.LogInformation("Consumer stopped");
    }
}