using UQ.Api.Domain.Partial;

namespace UQ.Api.Application.Consumer;

public interface IRetryConsumer
{
    public Task ExecuteCall(MinimalMessageToRetryData data);
}