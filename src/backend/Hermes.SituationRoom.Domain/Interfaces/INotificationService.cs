using Hermes.SituationRoom.Shared.BusinessObjects;

namespace Hermes.SituationRoom.Domain.Interfaces;

public interface INotificationService
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
    
    // Notification creation helpers
    Task<NotificationBo> CreateMessageNotificationAsync(Guid userUid, string senderName, Guid chatUid);
    Task<NotificationBo> CreateCommentNotificationAsync(Guid userUid, string commenterName, Guid postUid);
    Task<NotificationBo> CreateReplyNotificationAsync(Guid userUid, string replierName, Guid commentUid, Guid postUid);
    Task<NotificationBo> CreateLikeNotificationAsync(Guid userUid, string likerName, Guid postUid);
}
