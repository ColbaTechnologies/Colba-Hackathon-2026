using UQ.Api.Domain;

namespace UQ.Api.Application.Repositories;

public interface IMessageRepository
{
    public Task<MessageState?> GetMessageState(string publicId);
}