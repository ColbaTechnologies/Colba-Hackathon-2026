using Quartz;

namespace UQ.Api.Infrastructure.Quartz;

public class RetryJobConfig : IJobConfiguration
{
        public string Schedule { get; set; } = "0 1 0 * * ?";
        public string JobName { get; set; } = nameof(RetryJobConfig);
}