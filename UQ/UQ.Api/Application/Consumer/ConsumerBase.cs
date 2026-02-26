using Microsoft.EntityFrameworkCore;
using UQ.Api.Domain;
using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure;
using UQ.Api.Infrastructure.MessageModels;

namespace UQ.Api.Application.Consumer;

public class ConsumerBase
{
    public static async Task<Message> GetMessageFromData(IAppDbContext dbContext, MinimalMessageData data)
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
        
        return message;
    }
    
    protected static void FromMinimalToFailed(IAppDbContext dbContext, MinimalMessage minimalMessage) 
    {
        dbContext.MinimalMessages.Remove(minimalMessage);
        var minimalToRetry = minimalMessage.ToRetry();
        dbContext.FailedMessages.Add(minimalToRetry.ToFailed());
    }
    
    protected static void FromMinimalToRetry(IAppDbContext dbContext, MinimalMessage minimalMessage) 
    {
        dbContext.MinimalMessages.Remove(minimalMessage);
        var minimalToRetry = minimalMessage.ToRetry();
        dbContext.MinimalMessagesToRetry.Add(minimalToRetry);
    }
    
    protected static void FromRetryToFailed(IAppDbContext dbContext, MinimalMessageToRetry minimalMessage) 
    {
        dbContext.MinimalMessagesToRetry.Remove(minimalMessage);
        dbContext.FailedMessages.Add(minimalMessage.ToFailed(minimalMessage.RetryCount));
    }
}