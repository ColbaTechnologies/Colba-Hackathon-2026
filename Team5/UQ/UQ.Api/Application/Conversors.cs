using UQ.Api.Domain;
using UQ.Api.Domain.Partial;
using UQ.Api.Infrastructure.MessageModels;
using UQ.Api.Presentation.Dtos;

namespace UQ.Api.Application;

public static class Conversors
{
    public static CreateMessageInput ToMessageInput(this MessageInput messageInput)
    {
        return new CreateMessageInput(messageInput.DestinationUrl, messageInput.Headers, messageInput.Body,
            messageInput.ScheduledOn,
            messageInput.CallbackUrl,
            messageInput.CallerRequestId);
    }

    public static (MinimalMessage, MessageBody, List<MessageHeader>) ToDatabaseMessage(this Message message)
    {
        var minimalMessage = new MinimalMessage
        {
            Id = message.Id,
            PublicId = message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn?.UtcDateTime,
            CreatedAt = message.CreatedAt.DateTime,
            UpdatedAt = message.UpdatedAt.UtcDateTime,
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId
        };

        var messageBody = new MessageBody
        {
            BodyValue = message.Body,
            MessageId = message.Id
        };

        var messageHeaders =
            message.Headers.Select(x => new MessageHeader
            {
                MessageId = message.Id,
                HeaderKey = x.Key,
                HeaderValue = x.Value
            }).ToList();

        return (minimalMessage, messageBody, messageHeaders);
    }

    public static MinimalMessageData ToData(this MinimalMessage message)
    {
        return new MinimalMessageData
        {
            Id = message.Id,
            PublicId = message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn, // TODO: handle schedules
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId
        };
    }

    public static MinimalMessageToRetryData ToRetryData(this MinimalMessageToRetry message)
    {
        return new MinimalMessageToRetryData
        {
            Id = message.Id,
            PublicId = message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn, // TODO: handle schedules
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId,
            RetryCount = message.RetryCount,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }

    public static MinimalMessageToRetry ToRetry(this MinimalMessage message)
    {
        return new MinimalMessageToRetry
        {
            Id = message.Id,
            PublicId = message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn,
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId,
            RetryCount = 0,
            CreatedAt = message.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static FailedMessage ToFailed(this MinimalMessage message, int RetryCount = 0)
    {
        return new FailedMessage
        {
            Id = message.Id,
            PublicId = message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn,
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId,
            RetryCount = RetryCount,
            CreatedAt = message.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }
}