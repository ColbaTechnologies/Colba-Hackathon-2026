using MessagingSystem.Application.Interfaces;
using Microsoft.Extensions.Hosting;

namespace MessagingSystem.Application.Dispatchers;

public sealed class DeadLetterDispatcher(IMessageStore store, IMessageProcessor processor) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
     
            /*

            var messages = await store.GetPendingAsync(stoppingToken);

            foreach (var receivedMessage in messages)
            {
                await processor.ProcessAsync(receivedMessage, stoppingToken);
            }

            */
       
    }
}