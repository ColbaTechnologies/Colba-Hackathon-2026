using UQ.Api.Domain.Dtos;
using UQ.Api.Presentation.Dtos;

namespace UQ.Api.Application.Producer;

public interface IProducer
{
    public Task<SavePendingMessageResult> SavePendingMessage(MessageInput messageInput);
}