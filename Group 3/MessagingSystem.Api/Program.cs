using System.Text.Json.Serialization;
using MessagingSystem.Application.Configuration;
using MessagingSystem.Application.Dispatchers;
using MessagingSystem.Application.Interfaces;
using MessagingSystem.Application.Services;
using MessagingSystem.Infrastructure.Configuration;
using MessagingSystem.Infrastructure.Notifications;
using MessagingSystem.Infrastructure.Persistence;
using MessagingSystem.Infrastructure.RealTime;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
builder.Services.AddRavenDb(configuration);

builder.Services.AddHttpClient("default")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new SocketsHttpHandler
        {
            MaxConnectionsPerServer = 50
        });

builder.Services.Configure<RetrySettings>(
    builder.Configuration.GetSection("RetrySettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<RetrySettings>>().Value);

builder.Services.AddSingleton<ICallbackNotifier, HttpCallbackNotifier>();

builder.Services.AddSingleton<IMetricsPublisher, MetricsPublisher>();
builder.Services.AddSingleton<IMessageProcessor, MessageProcessor>();
builder.Services.AddSingleton<IMessageStore, MessageStore>();

builder.Services.AddHostedService<MessageDispatcher>();
builder.Services.AddHostedService<RetryDispatcher>();
builder.Services.AddHostedService<DeadLetterDispatcher>();

builder.Services.AddSignalR();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHub<MetricsHub>("/hubs/metrics");
app.UseHttpsRedirection();
app.Run();

