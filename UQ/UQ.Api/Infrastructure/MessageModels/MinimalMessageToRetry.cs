namespace UQ.Api.Infrastructure.MessageModels;

public class MinimalMessageToRetry : MinimalMessage
{
    public int RetryCount { get; set; } = 0;
}

public class FailedMessage : MinimalMessageToRetry
{
}