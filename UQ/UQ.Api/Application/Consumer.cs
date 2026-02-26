using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class Consumer(IAppDbContext dbContext, IHttpClientFactory httpClientFactory) : IConsumer
{
    public async Task ExecuteCall(MinimalMessageData data)
    {
        // load message parts
        
        // build message
        
        // send message
        var client = httpClientFactory.CreateClient();

        var response = await client.PostAsync(data.DestinationUrl, new MultipartContent()); // TODO: fill with real content
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        // callbacks
        
    }
}