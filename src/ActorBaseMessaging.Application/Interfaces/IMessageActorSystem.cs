namespace ActorBaseMessaging.Application.Interfaces;

using System.Text.Json;
using DTOs;

public interface IMessageActorSystem
{
    void Enqueue(string requestId, string targetUrl, JsonElement payload);
    Task<MessageStatusDto?> GetStatusAsync(string requestId);
    Task InitializeAsync();
}
