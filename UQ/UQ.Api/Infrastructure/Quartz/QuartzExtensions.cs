using Quartz;
using UQ.Api.Application;
using UQ.Api.Application.Jobs;

namespace UQ.Api.Infrastructure.Quartz;

public static class QuartzExtensions
{
    public static void AddQuartzJobs(this IServiceCollection services, IConfiguration? configuration)
    {
        var pollConfig = new PollJobConfig();
        var retryConfig = new RetryJobConfig();

        if (configuration is not null)
        {
            configuration.Bind(nameof(PollJobConfig), pollConfig);
            services.AddSingleton(pollConfig);
            configuration.Bind(nameof(RetryJobConfig), retryConfig);
            services.AddSingleton(retryConfig);
        }

        services
            .AddQuartz(quartz =>
            {
                quartz.AddQuartzJobAtStartup<PollJob>(pollConfig);
                quartz.AddQuartzJobAtStartup<RetryJob>(retryConfig);
            })
            .AddQuartzHostedService(opts =>
                opts.WaitForJobsToComplete = true);
    }

    private static IServiceCollectionQuartzConfigurator AddQuartzJob<T>(
        this IServiceCollectionQuartzConfigurator quartzConfigurator,
        IJobConfiguration jobConfig) where T : IJob
    {
        var jobKey = GetJobKey(jobConfig);
        var configurator = quartzConfigurator
            .AddJob<T>(opts => opts.WithIdentity(jobKey))
            .AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{jobKey.Name}-trigger")
                .WithCronSchedule(jobConfig.Schedule));
        return configurator;
    }

    private static IServiceCollectionQuartzConfigurator AddQuartzJobAtStartup<T>(
        this IServiceCollectionQuartzConfigurator quartzConfigurator,
        IJobConfiguration jobConfig) where T : IJob
    {
        quartzConfigurator.AddQuartzJob<T>(jobConfig)
            .AddTrigger(triggerConfig =>
            {
                var jobKey = GetJobKey(jobConfig);
                triggerConfig.ForJob(jobKey)
                    .WithIdentity($"{jobKey.Name}-startup-trigger")
                    .WithSimpleSchedule();
            });

        return quartzConfigurator;
    }

    private static JobKey GetJobKey(IJobConfiguration configuration)
    {
        return new JobKey(configuration.JobName);
    }
}