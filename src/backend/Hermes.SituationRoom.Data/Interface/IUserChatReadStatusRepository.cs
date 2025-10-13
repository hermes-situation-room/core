namespace Hermes.SituationRoom.Data.Interface;

using Shared.BusinessObjects;

public interface IUserChatReadStatusRepository
{
    Task<Guid> AddAsync(UserChatReadStatusBo newReadStatusBo);

    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid readStatusId);

    Task<UserChatReadStatusBo> GetReadStatusByUserAndChatAsync(Guid userId, Guid chatId);

    Task<UserChatReadStatusBo> UpdateAsync(Guid readStatusId);

    Task<UserChatReadStatusBo> UpdateByUserAndChatAsync(Guid userId, Guid chatId);
    
    Task DeleteAsync(Guid readStatusId);
}