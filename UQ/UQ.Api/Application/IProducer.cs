namespace UQ.Api.Application;

public interface IProducer
{
    public Task<bool> SavePendingMessage(InputEntry inputEntry);
}