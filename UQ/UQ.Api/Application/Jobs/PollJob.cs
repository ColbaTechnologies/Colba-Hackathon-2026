using Microsoft.EntityFrameworkCore;
using Quartz;
using UQ.Api.Application.Consumer;
using UQ.Api.Domain;
using UQ.Api.Infrastructure.Data;

namespace UQ.Api.Application.Jobs;

public class PollJob(IAppDbContext dbContext, IConsumer consumer) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var requestsToSend = await dbContext.MinimalMessages
            .Where(x => x.State == MessageState.Pending)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        // TODO: parallel
        // TODO: consumer factory
        foreach (var minimal in requestsToSend)
        {
            minimal.State = MessageState.Processing;
            minimal.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        foreach (var minimal in requestsToSend) await consumer.ExecuteCall(minimal.ToData());
    }
}