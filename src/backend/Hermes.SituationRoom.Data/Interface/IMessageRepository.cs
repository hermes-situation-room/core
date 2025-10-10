namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IMessageRepository
{
    Task<Guid> AddAsync(MessageBo newMessageBo);

    Task<MessageBo> GetMessageAsync(Guid messageId);

    Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId);

    Task<MessageBo> UpdateAsync(Guid messageId, string newContent);
    
    Task DeleteAsync(Guid messageId);
}