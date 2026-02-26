namespace MessagingSystem.Application.Dtos;

public sealed class BatchEnqueueRequest
{
    public List<EnqueueRequest> Items { get; set; } = new();
}