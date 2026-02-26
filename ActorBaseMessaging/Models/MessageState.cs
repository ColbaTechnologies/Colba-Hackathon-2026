namespace ActorBaseMessaging.Models;

public enum MessageState
{
    Pending,
    Retrying,
    Erroneous,
    Delivered
}
