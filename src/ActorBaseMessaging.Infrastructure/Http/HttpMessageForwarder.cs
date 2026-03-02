namespace ActorBaseMessaging.Infrastructure.Http;

using System.Text;
using Domain.Interfaces;

public sealed class HttpMessageForwarder(IHttpClientFactory httpClientFactory) : IMessageForwarder
{
    public async Task ForwardAsync(string targetUrl, string rawPayload)
    {
        var client  = httpClientFactory.CreateClient("MessageForwarder");
        var content = new StringContent(rawPayload, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(targetUrl, content);
        response.EnsureSuccessStatusCode();
    }
}
