using MessagingSystem.Application.Interfaces;

namespace MessagingSystem.Application.Services;

public class ProcessorIdentifier : IProcessorIdentifier
{
    public string InstanceId { get; } = $"{Environment.MachineName}-{Guid.NewGuid():N}";
}