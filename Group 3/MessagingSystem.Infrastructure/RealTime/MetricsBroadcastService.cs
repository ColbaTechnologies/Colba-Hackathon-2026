using MessagingSystem.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace MessagingSystem.Infrastructure.RealTime;

public sealed class MetricsBroadcastService(
    IMetricsPublisher metrics,
    IHubContext<MetricsHub> hubContext)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshot = metrics.GetSnapshot();

            await hubContext.Clients.All
                .SendAsync("metricsUpdated", snapshot, stoppingToken);

            await Task.Delay(2000, stoppingToken);
        }
    }
}