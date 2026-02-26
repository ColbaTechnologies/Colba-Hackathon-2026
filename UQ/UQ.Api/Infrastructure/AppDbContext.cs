using Microsoft.EntityFrameworkCore;
using UQ.Api.Infrastructure.MessageModels;

namespace UQ.Api.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<MinimalMessage> MinimalMessages => Set<MinimalMessage>();
    public DbSet<MessageHeader> MessageHeaders => Set<MessageHeader>();
    public DbSet<MessageBody> MessageBodies => Set<MessageBody>();
    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }
}