using Producer.Models;
using Raven.Client.Documents;

namespace Producer.Repository;

public class MessageRepository(IDocumentStore store) : IMessageRepository
{
    private async Task StoreAsync<T>(T message)
    {
        using var session = store.OpenAsyncSession();
        await session.StoreAsync(message);
        await session.SaveChangesAsync();
    }
    
    public async Task SaveReceivedMessageAsync(ReceivedMessage message)
    {
        await StoreAsync(message);
    }

    public async Task SaveProcessedMessageAsync(ProcessedMessage message)
    {
        await StoreAsync(message);
    }

    public async Task SaveProcessingMessageAsync(ProcessingMessage message)
    {
        await StoreAsync(message);    }

    public async Task SaveErrorMessageAsync(ErrorMessage message)
    {
        await StoreAsync(message);
    }
}