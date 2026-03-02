using MessagingSystem.Application.Dtos;
using MessagingSystem.Domain.Entities;

namespace MessagingSystem.Application.Interfaces;

public interface IMessageProcessor
{
    Task<MessageProcessingResult> ProcessAsync(ReceivedMessage receivedMessage, CancellationToken cancellationToken);
}