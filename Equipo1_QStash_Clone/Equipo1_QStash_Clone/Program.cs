using System.Diagnostics.Metrics;
using System.Threading.Channels;
using Equipo1_QStash_Clone.Model;
using Equipo1_QStash_Clone.Services;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Raven.Client.Documents;
using Serilog;

const string serverUrl = "http://127.0.0.1:8080";
const string databaseName = "MessageDB";

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();


builder.Host.UseSerilog(logger);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: builder.Environment.ApplicationName))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(exporter =>
        {
            exporter.Endpoint = new Uri("http://127.0.0.1:4317");
            exporter.Protocol = OtlpExportProtocol.Grpc;
            
        }));

        
//builder.Services.AddHostedService<Consumer>();
builder.Services.AddSingleton<QueueRepository>();
builder.Services.AddHostedService<ChannelSeeder>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDocumentStore>(_ =>
{
    var store = new DocumentStore
    {
        Urls = [serverUrl],
        Database = databaseName,
    };
    store.Initialize();
    return store;
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/health", () =>
    {
       
    })
    .WithName("health")
    .WithOpenApi();

app.MapPost("/publish", async (InputMessage inputMessage, QueueRepository queueRepository, IDocumentStore store) =>
{
    
    var channel = queueRepository.GetChannelQueue(inputMessage.QueueId);
    
    var currentMessage =new PersistedMessage
    {
        Id = Guid.NewGuid().ToString(),
        InputMessage = inputMessage,
        Timestamp = DateTime.UtcNow,
        QueueId = inputMessage.QueueId
    };
    using var session = store.OpenAsyncSession();
    await session.StoreAsync(currentMessage);
    await session.SaveChangesAsync();
    
    await channel.Writer.WriteAsync(currentMessage.Id);
    logger.Information("Message published {info}", channel.Reader.Count);
    return Results.Ok();
});

app.MapPost("/queue", async (string queueName, bool deathLetterEnable, IDocumentStore store, QueueRepository queueRepository, int retries = 3) =>
{
    var newQueue = new Queue
    {
        Id = Guid.NewGuid().ToString(),
        Name =  queueName.ToLowerInvariant(),
        Retries = retries,
    };

    if (deathLetterEnable)
    {
        newQueue.DeathLetterQueueId = Guid.NewGuid().ToString();
        newQueue.DeathLetterQueueName = $"DeathLetter{queueName}".ToLowerInvariant();
    }
    using var session = store.OpenAsyncSession();
    await session.StoreAsync(newQueue);
    await session.SaveChangesAsync();
    
    queueRepository.CreateQueue(newQueue.Id);

    return Results.Ok(newQueue);
});


app.MapDelete("/queue", async (string queueName, IDocumentStore store, QueueRepository queueRepository) =>
{
    using var session = store.OpenAsyncSession();
    var queue = await session.Query<Queue>().Where(x =>  x.Name == queueName).FirstOrDefaultAsync();
    
    queueRepository.DeleteQueue(queue.Id);
    session.Delete(queue);
    var messages = await session.Query<PersistedMessage>().Where(x => x.QueueId == queue.Id).ToListAsync();
    
    session.Delete(messages);
    await session.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/queue", async (string queueName, IDocumentStore store) =>
{
    using var session = store.OpenAsyncSession();
    var queue = await session.Query<Queue>().Where(x =>  x.Name == queueName).FirstOrDefaultAsync();
    await session.SaveChangesAsync();

    return Results.Ok(queue);
});

app.MapGet("/queue/clean", async (string queueName, IDocumentStore store,QueueRepository queueRepository) =>
{
    
    using var session = store.OpenAsyncSession();
    var queue = await session.Query<Queue>().Where(x =>  x.Name == queueName).FirstOrDefaultAsync();
    var totalMessage = queueRepository.DeleteMessages(queue.Id);    

    return Results.Ok(totalMessage);
});

app.MapGet("/queue/messages", async (string queueName, IDocumentStore store,QueueRepository queueRepository) =>
{
    
    using var session = store.OpenAsyncSession();
    var queue = await session.Query<Queue>().Where(x =>  x.Name == queueName).FirstOrDefaultAsync();
    var messages = await session.Query<PersistedMessage>().Where(x =>  x.QueueId == queue.Id).ToListAsync();
    
    return Results.Ok(messages.Count);
});



app.MapPost("/reciver", (HttpRequest request) =>
{
    logger.Information("Received request {headers}", request.Headers);
    logger.Information("Received body {body}", request.Body);
    return Results.Ok();
});

app.Run();