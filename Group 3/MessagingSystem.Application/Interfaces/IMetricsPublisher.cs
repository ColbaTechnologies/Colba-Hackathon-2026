using MessagingSystem.Application.Dtos;

namespace MessagingSystem.Application.Interfaces;

public interface IMetricsPublisher
{
    void IncrementProcessed();
    void IncrementFailed();
    MetricsSnapshot GetSnapshot();
}