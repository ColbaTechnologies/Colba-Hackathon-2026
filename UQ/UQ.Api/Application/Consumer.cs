using Microsoft.EntityFrameworkCore;
using UQ.Api.Domain;
using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class Consumer(IAppDbContext dbContext, IHttpClientFactory httpClientFactory) : IConsumer
{
    public async Task ExecuteCall(MinimalMessageData data)
    {
        // load message parts
        /*
        var headers = await dbContext.MessageHeaders.Where(x => x.MessageId == data.Id).ToListAsync();
        var body = await dbContext.MessageBodies.FirstOrDefaultAsync(x => x.MessageId == data.Id);

        var message = new Message(new ExistingMessageInput( 
            data.Id,
            data.PublicId,
            data.DestinationUrl,
            headers.ToDictionary(header => header.HeaderKey, header => header.HeaderValue),
            body?.BodyValue ?? String.Empty,
            data.State,
            data.CreatedAt,
            data.UpdatedAt
            ));
        
        */
        // build message
        
        
        // send message
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsync(data.DestinationUrl, new MultipartContent()); // TODO: fill with real content
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        // callbacks
        
    }
}