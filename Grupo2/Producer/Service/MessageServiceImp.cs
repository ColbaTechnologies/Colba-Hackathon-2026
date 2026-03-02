using Producer.Models;
using Producer.Repository;

namespace Producer.Service;

public class MessageServiceImp(IMessageRepository repository) : IMessageService
{
    public async Task<bool> ProcessMessageAsync(string payload, string destinationUrl)
    {
        var message = new ReceivedMessage
        {
            Id = Guid.NewGuid().ToString(),
            Payload = payload,
            DestinationUrl = destinationUrl,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await repository.SaveReceivedMessageAsync(message);
        }
        catch (Exception)
        {
            return false;
        }
        
        return true;
    }
}