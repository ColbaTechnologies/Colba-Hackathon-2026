using UQ.Api.Domain.Partial;

namespace UQ.Api.Application;

public interface IConsumer
{
    public Task ExecuteCall(MinimalMessageData data);
}