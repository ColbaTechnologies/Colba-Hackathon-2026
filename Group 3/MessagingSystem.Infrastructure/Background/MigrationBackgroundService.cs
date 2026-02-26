using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Migrations;

namespace MessagingSystem.Infrastructure.Background;

    public class MigrationBackgroundService(
        IEnumerable<MigrationRunner> runners,
        ILogger<MigrationBackgroundService> logger)
        : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (runners == null || !runners.Any())
            {
                logger.LogWarning("No migration runners were found.");
                return Task.CompletedTask;
            }

            foreach (var runner in runners)
            {
                var store = runner.GetStoreName();
                try
                {
                    logger.LogInformation("Starting migration with runner for database: {RunnerName}", store);
                    runner.Run();
                    logger.LogInformation("Successfully completed migration with runner for database: {RunnerName}", store);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while running the migration with runner for database: {RunnerName}", store);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("MigrationBackgroundService is stopping.");
            return Task.CompletedTask;
        }
    }


    public static class MigrationRunnerExtensions
    {
        public static string GetStoreName(this MigrationRunner runner)
        {
            var storeField = typeof(MigrationRunner).GetField("store", BindingFlags.NonPublic | BindingFlags.Instance);
            var store = storeField?.GetValue(runner) as IDocumentStore;
            return store?.Database ?? "Unknown";
        }
    }
