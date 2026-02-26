using UQ.Api.Domain.Dtos;

namespace UQ.Api.Application.Producer;

public interface IProducer
{
    public Task<SavePendingMessageResult> SavePendingMessage(InputEntry inputEntry);
}