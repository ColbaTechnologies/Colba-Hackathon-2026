namespace UQ.Api.Domain;

public enum MessageState
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Processing = 3,
    ToRetry = 4,
    Retrying = 5
}