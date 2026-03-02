using System.Text.Json.Serialization;
using ActorBaseMessaging.Models;
using ActorBaseMessaging.Services;
using Proto;
using Raven.Client.Documents;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddHttpClient("MessageForwarder", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.ConfigureHttpJsonOptions(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton<ActorSystem>();

// RavenDB document store — configured from appsettings.json "RavenDb" section
builder.Services.AddSingleton<IDocumentStore>(sp =>
{
    var cfg      = builder.Configuration.GetSection("RavenDb");
    var urls     = cfg.GetSection("Urls").Get<string[]>()
                   ?? throw new InvalidOperationException("RavenDb:Urls is not configured.");
    var database = cfg["DatabaseName"]
                   ?? throw new InvalidOperationException("RavenDb:DatabaseName is not configured.");

    var store = new DocumentStore { Urls = urls, Database = database };
    store.Initialize();
    return store;
});

builder.Services.AddSingleton<MessageActorSystem>();

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Run crash recovery before the HTTP server starts accepting requests.
var messagingSystem = app.Services.GetRequiredService<MessageActorSystem>();
await messagingSystem.InitializeAsync();

// ── Endpoints ─────────────────────────────────────────────────────────────────

app.MapPost("/messages", (InboundRequest request, MessageActorSystem actorSystem) =>
{
    if (string.IsNullOrWhiteSpace(request.TargetUrl))
        return Results.BadRequest(new { error = "targetUrl is required." });

    var requestId = Guid.NewGuid().ToString("N");
    actorSystem.Enqueue(requestId, request.TargetUrl, request.Payload);

    return Results.Accepted($"/messages/{requestId}", new { requestId });
});

app.MapGet("/messages/{requestId}", async (string requestId, MessageActorSystem actorSystem) =>
{
    var status = await actorSystem.GetStatusAsync(requestId);

    return status is null
        ? Results.NotFound(new { error = $"Request '{requestId}' not found." })
        : Results.Ok(status);
});

app.Run();
