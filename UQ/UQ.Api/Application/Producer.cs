using UQ.Api.Domain;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class Producer(IAppDbContext dbContext) : IProducer
{
    public async Task<bool> SavePendingMessage(InputEntry inputEntry)
    {
        // TODO: validations
        
        var message = new Message(inputEntry.ToMessageInput());

        var (minimalMessage, messageBody, messageHeaders) = message.ToDatabaseMessage();
        await dbContext.MinimalMessages.AddAsync(minimalMessage);
        await dbContext.MessageBodies.AddAsync(messageBody);
        await dbContext.MessageHeaders.AddRangeAsync(messageHeaders);
        await dbContext.SaveChangesAsync();
        
        return true;
    }
}