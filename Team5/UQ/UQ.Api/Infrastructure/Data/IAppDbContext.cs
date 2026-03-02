using Microsoft.EntityFrameworkCore;
using UQ.Api.Infrastructure.MessageModels;

namespace UQ.Api.Infrastructure.Data;

public interface IAppDbContext : IDisposable
{
    public DbSet<MinimalMessage> MinimalMessages { get; }
    public DbSet<MinimalMessageToRetry> MinimalMessagesToRetry { get; }
    public DbSet<FailedMessage> FailedMessages { get; }
    public DbSet<MessageHeader> MessageHeaders { get; }
    public DbSet<MessageBody> MessageBodies { get; }

    public Task<int> SaveChangesAsync();
}