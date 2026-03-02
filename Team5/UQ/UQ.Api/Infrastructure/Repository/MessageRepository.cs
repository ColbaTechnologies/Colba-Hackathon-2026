using Microsoft.EntityFrameworkCore;
using UQ.Api.Application.Repositories;
using UQ.Api.Domain;
using UQ.Api.Infrastructure.Data;

namespace UQ.Api.Infrastructure.Repository;

public class MessageRepository(IAppDbContext dbContext) : IMessageRepository
{
    public async Task<MessageState?> GetMessageState(string publicId)
    {
        var minimalMessage = await dbContext.MinimalMessages.FirstOrDefaultAsync(x => x.PublicId == publicId);
        return minimalMessage?.State;
    }
}