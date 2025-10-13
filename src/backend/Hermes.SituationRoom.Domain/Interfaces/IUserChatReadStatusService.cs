using Hermes.SituationRoom.Shared.BusinessObjects;

namespace Hermes.SituationRoom.Domain.Interfaces;

public interface IUserChatReadStatusService
{
    Task CreateReadStatusAsync(Guid userId, Guid chatId);
    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid uid);
    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid userId, Guid chatId);
    Task<int> GetUnreadMessageCountAsync(Guid uid);
    Task<int> GetUnreadMessageCountAsync(Guid userId, Guid chatId);
    Task<UserChatReadStatusBo> UpdateReadStatusAsync(UserChatReadStatusBo userChatReadStatus);
    Task<UserChatReadStatusBo> UpdateReadStatusAsync(Guid userId, Guid chatId);
    Task DeleteReadStatusAsync(Guid uid);
}
