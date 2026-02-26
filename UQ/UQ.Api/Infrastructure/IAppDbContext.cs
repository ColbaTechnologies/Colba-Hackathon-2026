using Microsoft.EntityFrameworkCore;
using UQ.Api.Infrastructure.MessageModels;

namespace UQ.Api.Infrastructure;

public interface IAppDbContext : IDisposable
{
    public DbSet<MinimalMessage> MinimalMessages { get; }
    public DbSet<MinimalMessageToRetry> MinimalMessagesToRetry { get; }
    public DbSet<MinimalMessageToRetry> FailedMessages { get; }
    public DbSet<MessageHeader> MessageHeaders { get; }
    public DbSet<MessageBody> MessageBodies { get; }
    
    public Task<int> SaveChangesAsync();
}