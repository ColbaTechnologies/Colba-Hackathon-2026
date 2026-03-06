using ActorBaseMessaging.Domain.Interfaces;
using ActorBaseMessaging.Infrastructure.Persistence;
using ActorBaseMessaging.Initializer;
using Raven.Client.Documents;
using Raven.Client.Exceptions;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var cfg      = ctx.Configuration.GetSection("RavenDb");
        var urls     = cfg.GetSection("Urls").Get<string[]>()
                       ?? throw new InvalidOperationException("RavenDb:Urls is not configured.");
        var database = cfg["DatabaseName"]
                       ?? throw new InvalidOperationException("RavenDb:DatabaseName is not configured.");

        services.AddSingleton<IDocumentStore>(_ =>
        {
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

        services.AddSingleton<IMessageRepository, RavenDbMessageRepository>();
        services.AddHttpClient("ApiClient");
        services.AddHostedService<PendingMessageRelayService>();
    })
    .Build();

await host.RunAsync();
