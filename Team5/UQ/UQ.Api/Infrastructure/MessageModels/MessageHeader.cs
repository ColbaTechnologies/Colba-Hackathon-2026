namespace UQ.Api.Infrastructure.MessageModels;

public class MessageHeader
{
    public long Id { get; set; }
    public string MessageId { get; set; }
    public string HeaderKey { get; set; }
    public string HeaderValue { get; set; }
}