#nullable enable
namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IMessageService
{
    Task<Guid> AddAsync(NewMessageDto newMessage);

    Task<MessageBo> GetMessageAsync(Guid messageId);

    Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId);

    Task UpdateAsync(Guid messageId, string newContent);
    
    Task DeleteAsync(Guid messageId);
}
