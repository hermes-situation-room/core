namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IUserChatReadStatusRepository
{
    Task<Guid> AddAsync(UserChatReadStatusBo newReadStatusBo);

    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid readStatusId);
    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid userId, Guid chatId);

    Task<UserChatReadStatusBo> UpdateAsync(Guid readStatusId);
    Task<UserChatReadStatusBo> UpdateAsync(Guid userId, Guid chatId);
    
    Task DeleteAsync(Guid readStatusId);
    
    Task<int> GetUnreadMessagesCountAsync(Guid userId);
    Task<int> GetUnreadMessagesCountAsync(Guid userId, Guid chatId);
}