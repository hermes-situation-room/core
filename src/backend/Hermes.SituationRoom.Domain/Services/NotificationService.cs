using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Microsoft.AspNetCore.SignalR;
using Hermes.SituationRoom.Domain.Hubs;

namespace Hermes.SituationRoom.Domain.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IHubContext<ChatHub> _hubContext;

    public NotificationService(INotificationRepository notificationRepository, IHubContext<ChatHub> hubContext)
    {
        _notificationRepository = notificationRepository;
        _hubContext = hubContext;
    }

    public async Task<NotificationBo?> GetByIdAsync(Guid uid)
    {
        return await _notificationRepository.GetByIdAsync(uid);
    }

    public async Task<IEnumerable<NotificationBo>> GetByUserUidAsync(Guid userUid)
    {
        return await _notificationRepository.GetByUserUidAsync(userUid);
    }

    public async Task<IEnumerable<NotificationBo>> GetUnreadByUserUidAsync(Guid userUid)
    {
        return await _notificationRepository.GetUnreadByUserUidAsync(userUid);
    }

    public async Task<NotificationBo> CreateAsync(NotificationBo notification)
    {
        var createdNotification = await _notificationRepository.CreateAsync(notification);
        
        // Send real-time notification to the user
        try
        {
            await _hubContext.Clients.Group(notification.UserUid.ToString())
                .SendAsync("ReceiveNotification", createdNotification);
        }
        catch (Exception)
        {
            // Don't fail notification creation if SignalR fails
        }
        
        return createdNotification;
    }

    public async Task<NotificationBo> UpdateAsync(NotificationBo notification)
    {
        return await _notificationRepository.UpdateAsync(notification);
    }

    public async Task DeleteAsync(Guid uid)
    {
        await _notificationRepository.DeleteAsync(uid);
    }

    public async Task MarkAsReadAsync(Guid uid)
    {
        await _notificationRepository.MarkAsReadAsync(uid);
    }

    public async Task MarkAllAsReadAsync(Guid userUid)
    {
        await _notificationRepository.MarkAllAsReadAsync(userUid);
    }

    public async Task<int> GetUnreadCountAsync(Guid userUid)
    {
        return await _notificationRepository.GetUnreadCountAsync(userUid);
    }

    public async Task<NotificationBo> CreateMessageNotificationAsync(Guid userUid, string senderName, Guid chatUid)
    {
        var notification = new NotificationBo(
            Guid.NewGuid(),
            userUid,
            "New Message",
            $"{senderName} sent you a message",
            "message",
            false,
            DateTime.UtcNow,
            chatUid,
            "chat"
        );

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<NotificationBo> CreateCommentNotificationAsync(Guid userUid, string commenterName, Guid postUid)
    {
        var notification = new NotificationBo(
            Guid.NewGuid(),
            userUid,
            "New Comment",
            $"{commenterName} commented on your post",
            "comment",
            false,
            DateTime.UtcNow,
            postUid,
            "post"
        );

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<NotificationBo> CreateReplyNotificationAsync(Guid userUid, string replierName, Guid commentUid, Guid postUid)
    {
        var notification = new NotificationBo(
            Guid.NewGuid(),
            userUid,
            "New Reply",
            $"{replierName} replied to your comment",
            "reply",
            false,
            DateTime.UtcNow,
            commentUid,
            "comment"
        );

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<NotificationBo> CreateLikeNotificationAsync(Guid userUid, string likerName, Guid postUid)
    {
        var notification = new NotificationBo(
            Guid.NewGuid(),
            userUid,
            "Post Liked",
            $"{likerName} liked your post",
            "like",
            false,
            DateTime.UtcNow,
            postUid,
            "post"
        );

        return await _notificationRepository.CreateAsync(notification);
    }
}
