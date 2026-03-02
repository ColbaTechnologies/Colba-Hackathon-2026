using Producer.Repository;
using Producer.Service;
using Producer.Worker;
using Raven.Client.Documents;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Runtime;

var builder = WebApplication.CreateBuilder(args);

// Service metadata
var serviceName = "producer-server";
var serviceVersion = "1.0.0";
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT") 
                   ?? "http://otel-collector:4317";

// ----------------------
// OpenTelemetry SDK setup (Traces + Metrics + Logging)
// ----------------------
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .WithTracing(tracing =>
    {
        tracing.AddSource(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    })
    .WithMetrics(metrics =>
    {
        metrics.AddMeter(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(serviceName, serviceVersion));
    logging.AddConsoleExporter();
    logging.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
});

// Keep console logging too
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// ----------------------
// Application services
// ----------------------
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageService, MessageServiceImp>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddTransient<IDocumentWorker, DocumentWorker>();
builder.Services.AddSingleton<IDocumentWorkerFactory, DocumentWorkerFactory>();
builder.Services.AddHostedService<ProcessingJob>();

// ----------------------
// RavenDB setup
// ----------------------
builder.Services.AddHostedService<ErrorMessageReprocessingJob>();
builder.Services.AddSingleton<IDocumentStore>(_ =>
{
    var storeUrl = Environment.GetEnvironmentVariable("RAVEN_URL") ?? "http://localhost:8080";
    var databaseName = Environment.GetEnvironmentVariable("RAVEN_DATABASE") ?? "Hackathon";
    
    var store = new DocumentStore
    {
        Urls = [storeUrl],
        Database = databaseName
    };
    
    store.Initialize();
    // Create the database if it doesn't exist
    try
    {
        store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(databaseName)));
    }
    catch (ConcurrencyException)
    {
        // Already exists database
    }
    
    return store;
});

// ----------------------
// Build & run app
// ----------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();