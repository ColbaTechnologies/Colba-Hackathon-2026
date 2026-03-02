using Producer.Extensions;
using Producer.Models;
using Raven.Client.Documents;

namespace Producer.Worker;

public interface IDocumentWorker
{
    Task ProcessAsync(string messageId, CancellationToken ct);
}

public class DocumentWorker(IDocumentStore store, IHttpClientFactory httpClientFactory, ILogger<DocumentWorker> logger)
    : IDocumentWorker
{
    public async Task ProcessAsync(string messageId, CancellationToken ct)
    {
        using var session = store.OpenAsyncSession();
        var processingMessage = new ProcessingMessage(); 
        try
        {
            // Load Received message
            var message = await session.LoadAsync<ReceivedMessage>(messageId, ct);
            
            if (message is null)
            {
                logger.LogWarning("Message with id: {Id} not found", messageId);
                return;
            }

            // Create the ProcessingMessage
            processingMessage = message.ToProcessingMessage();
            await session.StoreAsync(processingMessage, ct);
            
            // Delete ReceivedMessage
            session.Delete(message);
            await session.SaveChangesAsync(ct);

            // Do Post Request
            var client = httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync(
                message.DestinationUrl, 
                message.Payload, 
                cancellationToken: ct
            );
            response.EnsureSuccessStatusCode();

            // Flag as processed
            var processedMessage = new ProcessedMessage
            {
                Id = message.Id,
                Payload = message.Payload,
                DestinationUrl = message.DestinationUrl
            };
            
            await session.StoreAsync(processedMessage, ct);
            session.Delete(processingMessage.Id);
            await session.SaveChangesAsync(ct);

            logger.LogInformation("Message with id: {Id} correctly processed", message.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing message with id: {Id}", messageId);

            var errorMessage = new ErrorMessage
            {
                Id = Guid.NewGuid().ToString(),
                Payload = processingMessage.Payload,
                DestinationUrl = processingMessage.DestinationUrl
            };
            
            await session.StoreAsync(errorMessage, ct);
            session.Delete(processingMessage.Id);
            await session.SaveChangesAsync(ct);
        }
    }
}