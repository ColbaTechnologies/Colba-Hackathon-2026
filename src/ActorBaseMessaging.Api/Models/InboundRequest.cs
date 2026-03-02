namespace ActorBaseMessaging.Api.Models;

using System.Text.Json;

/// <summary>
/// The HTTP request body received by the API.
/// Payload is forwarded as-is to the TargetUrl.
/// </summary>
public record InboundRequest(string TargetUrl, JsonElement Payload);
