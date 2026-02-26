using Microsoft.EntityFrameworkCore;
using UQ.Api.Domain;
using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure;

namespace UQ.Api.Application.Consumer;

public class RetryConsumer(IAppDbContext dbContext, IHttpClientFactory httpClientFactory, ILogger<Consumer> logger)
    : ConsumerBase, IRetryConsumer
{
    private const int MaxRetryCount = 3;

    public async Task ExecuteCall(MinimalMessageToRetryData data)
    {
        var message = await GetMessageFromData(dbContext, data);

        var client = httpClientFactory.CreateClient();
        foreach (var header in message.Headers) client.DefaultRequestHeaders.Add(header.Key, header.Value);

        var minimalMessage = await dbContext.MinimalMessagesToRetry.FirstOrDefaultAsync(m => m.Id == data.Id);

        if (minimalMessage is null)
        {
            logger.LogWarning("Called consumer for non existing message with {DataId}", data.Id);
            return;
        }

        try
        {
            var response =
                await client.PostAsync(data.DestinationUrl,
                    new StringContent(message.Body));

            minimalMessage.State = response.IsSuccessStatusCode
                ? MessageState.Sent
                : minimalMessage.RetryCount > MaxRetryCount
                    ? MessageState.Failed
                    : MessageState.ToRetry;

            if (minimalMessage.State == MessageState.Failed) FromRetryToFailed(dbContext, minimalMessage);
            if (minimalMessage.State == MessageState.Sent) FromRetryToMinimal(dbContext, minimalMessage);
        }
        catch (Exception e)
        {
            minimalMessage.State = MessageState.Failed;
            logger.LogError("Retry Consumer failed");
            FromRetryToFailed(dbContext, minimalMessage);
        }

        minimalMessage.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        // TODO: callbacks
    }
}