using System.Text;
using MessagingSystem.Application.Configuration;
using MessagingSystem.Application.Dtos;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Domain.Entities;
using MessagingSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MessagingSystem.Application.Services;

public sealed class MessageProcessor(
    RetrySettings settings,
    IHttpClientFactory httpClientFactory,
    IMetricsPublisher metrics,
    ILogger<MessageProcessor> logger)
    : IMessageProcessor
{
    public async Task<MessageProcessingResult> ProcessAsync(
        ReceivedMessage receivedMessage,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient(); //

        var attemptCount = receivedMessage.AttemptCount + 1;
        var attemptTime = DateTimeOffset.UtcNow;

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                receivedMessage.DestinationUrl);

            if (!string.IsNullOrWhiteSpace(receivedMessage.SerializedPayload))
            {
                request.Content = new StringContent(
                    receivedMessage.SerializedPayload,
                    Encoding.UTF8,
                    "application/json");
            }

            foreach (var header in receivedMessage.Headers)
            {
                request.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value);
            }

            var response =
                await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                metrics.IncrementProcessed();
                return new MessageProcessingResult(
                    Status: MessageStatus.Delivered,
                    AttemptCount: attemptCount,
                    LastAttemptAtUtc: attemptTime,
                    NextAttemptAtUtc: null,
                    LastError: null);
            }

            metrics.IncrementFailed();
            return BuildRetryResult(attemptCount,
                attemptTime,
                $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return BuildRetryResult(attemptCount,
                attemptTime,
                ex.Message);
        }
    }

    private MessageProcessingResult BuildRetryResult(int attemptCount,
        DateTimeOffset attemptTime,
        string error)
    {
        if (attemptCount >= settings.MaxAttempts)
        {
            return new MessageProcessingResult(
                Status: MessageStatus.Failed,
                AttemptCount: attemptCount,
                LastAttemptAtUtc: attemptTime,
                NextAttemptAtUtc: null,
                LastError: error);
        }

        return new MessageProcessingResult(
            Status: MessageStatus.Retry,
            AttemptCount: attemptCount,
            LastAttemptAtUtc: attemptTime,
            NextAttemptAtUtc: attemptTime.Add(settings.Delay),
            LastError: error);
    }
}