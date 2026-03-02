namespace Producer.Service;

public interface IMessageService
{
    Task<bool> ProcessMessageAsync(string payload, string destinationUrl);
}