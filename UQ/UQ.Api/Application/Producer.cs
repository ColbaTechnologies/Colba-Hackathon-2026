using UQ.Api.Domain;
using UQ.Api.Domain.Dtos;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class Producer(IAppDbContext dbContext) : IProducer
{
    public async Task<SavePendingMessageResult> SavePendingMessage(InputEntry inputEntry)
    {
        var message = new Message(inputEntry.ToMessageInput());

        var (minimalMessage, messageBody, messageHeaders) = message.ToDatabaseMessage();
        await dbContext.MinimalMessages.AddAsync(minimalMessage);
        await dbContext.MessageBodies.AddAsync(messageBody);
        await dbContext.MessageHeaders.AddRangeAsync(messageHeaders);
        await dbContext.SaveChangesAsync();
        
        return new SavePendingMessageResult(true, message.PublicId);
    }
}