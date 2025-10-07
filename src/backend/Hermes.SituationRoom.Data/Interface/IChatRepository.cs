namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IChatRepository
{
    Task<Guid> AddAsync(ChatBo newChatBo);

    Task<ChatBo> GetChatAsync(Guid chatId);

    Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id);

    Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId);
    
    Task DeleteAsync(Guid chatId);
}

