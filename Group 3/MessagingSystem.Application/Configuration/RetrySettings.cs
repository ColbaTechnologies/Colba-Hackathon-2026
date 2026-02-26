namespace MessagingSystem.Application.Configuration;

public record RetrySettings
{
    public RetrySettings()
    {
    }

    public RetrySettings(int MaxAttempts, TimeSpan Delay)
    {
        this.MaxAttempts = MaxAttempts;
        this.Delay = Delay;
    }

    public int MaxAttempts { get; init; }
    public TimeSpan Delay { get; init; }
    
    public int BatchSize { get; init; }

    public int MaxParallelism { get; init; } = 20;

    public void Deconstruct(out int MaxAttempts, out TimeSpan Delay)
    {
        MaxAttempts = this.MaxAttempts;
        Delay = this.Delay;
    }
}