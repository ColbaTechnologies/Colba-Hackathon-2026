namespace UQ.Api.Infrastructure.Quartz;

public interface IJobConfiguration
{
    public string Schedule { get; set; }
    public string JobName { get; set; }
}