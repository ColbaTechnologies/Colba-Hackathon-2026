using UQ.Api.Domain.Partial;

namespace UQ.Api.Application.Consumer;

public interface IConsumer
{
    public Task ExecuteCall(MinimalMessageData data);
}