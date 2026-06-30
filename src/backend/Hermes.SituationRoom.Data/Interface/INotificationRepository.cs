using Hermes.SituationRoom.Shared.BusinessObjects;

namespace Hermes.SituationRoom.Data.Interface;

public interface INotificationRepository
{
    Task<NotificationBo?> GetByIdAsync(Guid uid);
    Task<IEnumerable<NotificationBo>> GetByUserUidAsync(Guid userUid);
    Task<IEnumerable<NotificationBo>> GetUnreadByUserUidAsync(Guid userUid);
    Task<NotificationBo> CreateAsync(NotificationBo notification);
    Task<NotificationBo> UpdateAsync(NotificationBo notification);
    Task DeleteAsync(Guid uid);
    Task MarkAsReadAsync(Guid uid);
    Task MarkAllAsReadAsync(Guid userUid);
    Task<int> GetUnreadCountAsync(Guid userUid);
}
