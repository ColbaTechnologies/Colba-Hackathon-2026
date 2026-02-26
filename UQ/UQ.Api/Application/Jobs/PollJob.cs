using Microsoft.EntityFrameworkCore;
using Quartz;
using UQ.Api.Domain;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application;

public class PollJob(IAppDbContext dbContext, IConsumer consumer) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var requestsToSend = await dbContext.MinimalMessages
            .Where(x => x.State == MessageState.Pending)
            .ToListAsync();

        // TODO: parallel
        // TODO: consumer factory
        foreach (var minimal in requestsToSend)
        {
            await consumer.ExecuteCall(minimal.ToData());
            minimal.State = MessageState.Processing;
        }

        await dbContext.SaveChangesAsync();
    }
}