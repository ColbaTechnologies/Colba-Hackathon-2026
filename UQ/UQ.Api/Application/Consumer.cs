using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using UQ.Api.Domain;
using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class Consumer(IAppDbContext dbContext, IHttpClientFactory httpClientFactory) : IConsumer
{
    public async Task ExecuteCall(MinimalMessageData data)
    {
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
        
        var client = httpClientFactory.CreateClient();
        foreach (var header in headers)
        {
            client.DefaultRequestHeaders.Add(header.HeaderKey, header.HeaderValue);
        }
        
        var response = await client.PostAsync(data.DestinationUrl, new StringContent(message.Body)); // TODO: fill with real content
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        // TODO: callbacks
        
    }
}