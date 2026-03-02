using UQ.Api.Domain;
using UQ.Api.Domain.Dtos;
using UQ.Api.Infrastructure.Data;
using UQ.Api.Presentation.Dtos;

namespace UQ.Api.Application.Producer;

public class Producer(IAppDbContext dbContext) : IProducer
{
    public async Task<SavePendingMessageResult> SavePendingMessage(MessageInput messageInput)
    {
        var message = new Message(messageInput.ToMessageInput());

        var (minimalMessage, messageBody, messageHeaders) = message.ToDatabaseMessage();
        await dbContext.MinimalMessages.AddAsync(minimalMessage);
        await dbContext.MessageBodies.AddAsync(messageBody);
        await dbContext.MessageHeaders.AddRangeAsync(messageHeaders);
        await dbContext.SaveChangesAsync();

        return new SavePendingMessageResult(true, message.PublicId);
    }
}