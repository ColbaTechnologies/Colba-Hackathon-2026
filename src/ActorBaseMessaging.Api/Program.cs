using System.Text.Json.Serialization;
using ActorBaseMessaging.Api.Models;
using ActorBaseMessaging.Application.Interfaces;
using ActorBaseMessaging.Application.Services;
using ActorBaseMessaging.Domain.Interfaces;
using ActorBaseMessaging.Api.Services;
using ActorBaseMessaging.Infrastructure.Http;
using ActorBaseMessaging.Infrastructure.Persistence;
using Proto;
using Raven.Client.Documents;
using Raven.Client.ServerWide.Operations;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;

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

    try
    {
        store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(database)));
    }
    catch (ConcurrencyException)
    {
        // Database already exists — safe to ignore.
    }

    return store;
});

builder.Services.AddSingleton<IMessageRepository, RavenDbMessageRepository>();
builder.Services.AddSingleton<IMessageForwarder, HttpMessageForwarder>();
builder.Services.AddSingleton<IMessageActorSystem, MessageActorSystem>();
builder.Services.AddSingleton<ApiReadinessService>();

// ── Build ─────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Endpoints ─────────────────────────────────────────────────────────────────

app.MapGet("/health", () => Results.Ok());

app.MapPost("/messages", async (InboundRequest request, IMessageActorSystem actorSystem, ApiReadinessService readiness) =>
{
    if (!await readiness.EnsureReadyAsync())
        return Results.StatusCode(503);

    if (string.IsNullOrWhiteSpace(request.TargetUrl))
        return Results.BadRequest(new { error = "targetUrl is required." });

    var requestId = Guid.NewGuid().ToString("N");
    actorSystem.Enqueue(requestId, request.TargetUrl, request.Payload);

    return Results.Accepted($"/messages/{requestId}", new { requestId });
});

app.MapGet("/messages/{requestId}", async (string requestId, IMessageActorSystem actorSystem) =>
{
    var status = await actorSystem.GetStatusAsync(requestId);

    return status is null
        ? Results.NotFound(new { error = $"Request '{requestId}' not found." })
        : Results.Ok(status);
});

app.MapPost("/internal/requeue/{requestId}", (string requestId, InboundRequest req, IMessageActorSystem actorSystem) =>
{
    actorSystem.Enqueue(requestId, req.TargetUrl, req.Payload);
    return Results.Accepted();
});

app.Run();
