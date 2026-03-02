using System.Reflection;
using MessagingSystem.Domain.Entities;
using MessagingSystem.Infrastructure.Background;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Raven.Migrations;

namespace MessagingSystem.Infrastructure.Configuration;

public static class RavenDbExtensions
{
    private static DocumentConventions RavenConventions => new()
    {
        FindCollectionName = type =>
        {
            return type switch
            {
                not null when type == typeof(DeadLetterMessage) => "DeadLetterMessages",
                not null when type == typeof(RetryMessage) => "RetryMessages",
                not null when type == typeof(ReceivedMessage) => "Messages",
                not null when type == typeof(ProcessedMessage) => "ProcessedMessages",
                _ => DocumentConventions.DefaultGetCollectionName(type)
            };
        }

    };
    
    public static IServiceCollection AddRavenDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = default)
    {
        var dbSettings = configuration.GetRavenDbSettings(sectionName);
        var store = new DocumentStore
        {
            Urls = dbSettings.Urls,
            Database = dbSettings.DatabaseName,
            // Certificate = cert,
            Conventions = RavenConventions
        }.Initialize();

        IndexCreation.CreateIndexes(typeof(RavenDbExtensions).Assembly, store);

        return services
            .AddSingleton(_ => store)
            .AddRavenDbSingleInstanceMigrations([typeof(RavenDbExtensions).Assembly], true, store);
    }

    private static IServiceCollection AddRavenDbSingleInstanceMigrations(this IServiceCollection services,
        IReadOnlyCollection<Assembly> assemblies,
        bool autoApplyMigrations = true,
        IDocumentStore store = default,
        TimeSpan? simultaneousMigrationTimeout = null)
    {
        if (autoApplyMigrations)
        {
            services.AddHostedService<MigrationBackgroundService>();
        }

        services.AddRavenDbMigrations(configure =>
        {
            configure.PreventSimultaneousMigrations = true;
            configure.SimultaneousMigrationTimeout = simultaneousMigrationTimeout ?? TimeSpan.FromHours(2);
            configure.Assemblies.Add(assemblies.SingleOrDefault()!);
        }, store!);
        return services;
    }
}
