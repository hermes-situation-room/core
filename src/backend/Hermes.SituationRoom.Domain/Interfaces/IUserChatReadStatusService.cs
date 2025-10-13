using Hermes.SituationRoom.Shared.BusinessObjects;

namespace Hermes.SituationRoom.Domain.Interfaces;

public interface IUserChatReadStatusService
{
    Task<Guid> CreateReadStatusAsync(Guid userId, Guid chatId);
    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid uid);
    Task<UserChatReadStatusBo> GetReadStatusAsync(Guid userId, Guid chatId);
    Task<int> GetUnreadMessageCountAsync(Guid userId);
    Task<int> GetUnreadMessageCountAsync(Guid userId, Guid chatId);
    Task<UserChatReadStatusBo> UpdateReadStatusAsync(Guid readStatusId);
    Task<UserChatReadStatusBo> UpdateReadStatusAsync(Guid userId, Guid chatId);
    Task DeleteReadStatusAsync(Guid uid);
}
