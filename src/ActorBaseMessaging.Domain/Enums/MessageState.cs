namespace ActorBaseMessaging.Domain.Enums;

public enum MessageState
{
    Pending,
    Retrying,
    Erroneous,
    Delivered
}
