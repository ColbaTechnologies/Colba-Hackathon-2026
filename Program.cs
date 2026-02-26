using System.Text.Json.Serialization;
using ActorBaseMessaging.Models;
using ActorBaseMessaging.Services;
using Proto;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

// Named HttpClient with a reasonable timeout for outbound forwarding
builder.Services.AddHttpClient("MessageForwarder", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Serialize enums as strings in JSON responses
builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Proto.Actor system – singleton for the lifetime of the app
builder.Services.AddSingleton<ActorSystem>();

// Our registry / actor-system façade
builder.Services.AddSingleton<MessageActorSystem>();

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Eagerly resolve so the actor system is warm before the first request
app.Services.GetRequiredService<MessageActorSystem>();

// ── Endpoints ─────────────────────────────────────────────────────────────────

// POST /messages
// Body: { "targetUrl": "https://...", "payload": { ... } }
// Returns 202 Accepted with { "requestId": "<id>" }
// Location header points to the status endpoint.
app.MapPost("/messages", (InboundRequest request, MessageActorSystem actorSystem) =>
{
    if (string.IsNullOrWhiteSpace(request.TargetUrl))
        return Results.BadRequest(new { error = "targetUrl is required." });

    var requestId = actorSystem.Enqueue(request.TargetUrl, request.Payload);

    return Results.Accepted(
        $"/messages/{requestId}",
        new { requestId });
});

// GET /messages/{requestId}
// Returns the current status snapshot of the actor.
app.MapGet("/messages/{requestId}", async (string requestId, MessageActorSystem actorSystem) =>
{
    var status = await actorSystem.GetStatusAsync(requestId);

    return status is null
        ? Results.NotFound(new { error = $"Request '{requestId}' not found." })
        : Results.Ok(status);
});

app.Run();
