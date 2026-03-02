using UQ.Api.Infrastructure.Data;

namespace UQ.Api.Application.Consumer;

public interface IConsumerFactory
{
    public IConsumer GetConsumer();
    public IRetryConsumer GetRetryConsumer();
}

public class ConsumerFactory(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory) : IConsumerFactory
{ 
    public IConsumer GetConsumer()
    { 
        var dbContext = serviceProvider.GetRequiredService<IAppDbContext>();
        var client = httpClientFactory.CreateClient();
        var logger = serviceProvider.GetRequiredService<ILogger<Consumer>>();
        return new Consumer(dbContext, client, logger);
    }

    public IRetryConsumer GetRetryConsumer()
    {
        var dbContext = serviceProvider.GetRequiredService<IAppDbContext>();
        var client = httpClientFactory.CreateClient();
        var logger = serviceProvider.GetRequiredService<ILogger<Consumer>>();
        return new RetryConsumer(dbContext, client, logger);
    }
}