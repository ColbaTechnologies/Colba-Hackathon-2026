using UQ.Api.Domain.Dtos;

namespace UQ.Api.Application;

public interface IProducer
{
    public Task<SavePendingMessageResult> SavePendingMessage(InputEntry inputEntry);
}