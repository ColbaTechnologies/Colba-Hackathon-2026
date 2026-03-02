using Producer.Models;

namespace Producer.Repository;

public interface IMessageRepository
{
    Task SaveReceivedMessageAsync(ReceivedMessage message);
    Task SaveProcessedMessageAsync(ProcessedMessage message);
    Task SaveProcessingMessageAsync(ProcessingMessage message);
    Task SaveErrorMessageAsync(ErrorMessage message);
}