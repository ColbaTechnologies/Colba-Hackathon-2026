using System.Net.Http.Json;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MessagingSystem.Infrastructure.Notifications;

public sealed class HttpCallbackNotifier(
    IHttpClientFactory httpClientFactory, 
    ILogger<HttpCallbackNotifier> logger)
    : ICallbackNotifier
{
    public async Task NotifyAsync(
        ReceivedMessage message,
        string status,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(message.CallbackUrl))
            return;

        if (!Uri.TryCreate(
                message.CallbackUrl,
                UriKind.Absolute,
                out var uri))
        {
            logger.LogWarning(
                "Invalid callback URL for message {MessageId}",
                message.Id);
            return;
        }
        
        var client = httpClientFactory.CreateClient();

        var payload = new
        {
            messageId = message.Id,
            status,
            attemptCount = message.AttemptCount
        };

        await client.PostAsJsonAsync(
            message.CallbackUrl,
            payload,
            ct);
    }
}