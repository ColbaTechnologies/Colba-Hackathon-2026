using Microsoft.EntityFrameworkCore;
using UQ.Api.Application.Repositories;
using UQ.Api.Domain;

namespace UQ.Api.Infrastructure;

public class MessageRepository(IAppDbContext dbContext) : IMessageRepository
{
    public async Task<MessageState?> GetMessageState(string publicId)
    {
        var minimalMessage = await dbContext.MinimalMessages.FirstOrDefaultAsync(x => x.PublicId == publicId);
        return minimalMessage?.State;
    }
}