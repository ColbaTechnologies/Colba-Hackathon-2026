namespace ActorBaseMessaging.Domain.Interfaces;

public interface IMessageForwarder
{
    Task ForwardAsync(string targetUrl, string rawPayload);
}
