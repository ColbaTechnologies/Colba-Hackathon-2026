namespace MessagingSystem.Application.Dtos;

public sealed class MetricsSnapshot
{
    public int Processed { get; init; }
    public int Failed { get; init; }
}