namespace ActorBaseMessaging.Initializer;

using System.Net.Http.Json;
using System.Text.Json;
using ActorBaseMessaging.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class PendingMessageRelayService(
    IMessageRepository          repository,
    IHttpClientFactory          httpClientFactory,
    IConfiguration              configuration,
    IHostApplicationLifetime    lifetime,
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
                    JsonElement payload;
                    using (var jsonDoc = JsonDocument.Parse(req.RawPayload))
                        payload = jsonDoc.RootElement.Clone();

                    var response = await client.PostAsJsonAsync(
                        $"{apiBaseUrl.TrimEnd('/')}/internal/requeue/{req.Id}",
                        new { targetUrl = req.TargetUrl, payload },
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
            try
            {
                await repository.MarkRecoveryCompleteAsync();
                logger.LogInformation("Recovery-complete flag written to database.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to write recovery-complete flag to database.");
            }

            logger.LogInformation("PendingMessageRelayService finished. Stopping application.");
            lifetime.StopApplication();
        }
    }
}
