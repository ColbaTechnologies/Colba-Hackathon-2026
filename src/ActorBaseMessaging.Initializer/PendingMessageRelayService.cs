namespace ActorBaseMessaging.Initializer;

using System.Net.Http.Json;
using ActorBaseMessaging.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class PendingMessageRelayService(
    IMessageRepository    repository,
    IHttpClientFactory    httpClientFactory,
    IConfiguration        configuration,
    IHostApplicationLifetime lifetime,
    ILogger<PendingMessageRelayService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiBaseUrl = configuration["Initializer:ApiBaseUrl"]
                         ?? throw new InvalidOperationException("Initializer:ApiBaseUrl is not configured.");

        logger.LogInformation("PendingMessageRelayService starting. API base URL: {ApiBaseUrl}", apiBaseUrl);

        try
        {
            var inflight = await repository.GetInFlightAsync();
            logger.LogInformation("Found {Count} in-flight record(s) to requeue.", inflight.Count);

            var client = httpClientFactory.CreateClient("ApiClient");

            foreach (var req in inflight)
            {
                try
                {
                    var response = await client.PostAsJsonAsync(
                        $"{apiBaseUrl.TrimEnd('/')}/internal/requeue",
                        new { requestId = req.Id, targetUrl = req.TargetUrl, rawPayload = req.RawPayload },
                        stoppingToken);

                    if (response.IsSuccessStatusCode)
                        logger.LogInformation("Requeued {RequestId} successfully.", req.Id);
                    else
                        logger.LogWarning("Failed to requeue {RequestId}: HTTP {StatusCode}.", req.Id, (int)response.StatusCode);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error requeuing {RequestId}.", req.Id);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to query in-flight records from repository.");
        }
        finally
        {
            logger.LogInformation("PendingMessageRelayService finished. Stopping application.");
            lifetime.StopApplication();
        }
    }
}
