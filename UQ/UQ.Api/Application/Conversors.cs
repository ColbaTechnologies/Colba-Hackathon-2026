using UQ.Api.Domain;
using UQ.Api.Infrastructure.MessageModels;

namespace UQ.Api.Application;

public static class Conversors
{
    public static MessageInput ToMessageInput(this InputEntry input)
    {
        return new MessageInput(input.DestinationUri, input.Headers, input.Body, input.ScheduledOn, input.CallbackUrl,
            input.CallerRequestId);
    }

    public static (MinimalMessage, MessageBody, List<MessageHeader>) ToDatabaseMessage(this Message message)
    {
        var minimalMessage = new MinimalMessage()
        {
            Id = message.Id,
            PublicId =  message.PublicId,
            DestinationUrl = message.DestinationUrl,
            State = message.State,
            ScheduledOn = message.ScheduledOn?.UtcDateTime,
            CallbackUrl = message.CallbackUrl,
            CallerRequestId = message.CallerRequestId
        };
        
        var messageBody = new MessageBody()
        {
            BodyValue = message.Body,
            MessageId = message.Id
        };

        var messageHeaders =
            message.Headers.Select(x => new MessageHeader()
            {
                MessageId = message.Id,
                HeaderKey = x.Key,
                HeaderValue = x.Value
            }).ToList();
        
        return (minimalMessage, messageBody, messageHeaders);
    }
}