using Microsoft.EntityFrameworkCore;
using Hermes.SituationRoom.Data.Context;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;

namespace Hermes.SituationRoom.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly IHermessituationRoomContext _context;

    public NotificationRepository(IHermessituationRoomContext context)
    {
        _context = context;
    }

    public async Task<NotificationBo?> GetByIdAsync(Guid uid)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Uid == uid);

        return notification != null ? MapToBo(notification) : null;
    }

    public async Task<IEnumerable<NotificationBo>> GetByUserUidAsync(Guid userUid)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserUid == userUid)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToBo);
    }

    public async Task<IEnumerable<NotificationBo>> GetUnreadByUserUidAsync(Guid userUid)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserUid == userUid && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToBo);
    }

    public async Task<NotificationBo> CreateAsync(NotificationBo notification)
    {
        var entity = new Entities.Notification
        {
            Uid = notification.Uid,
            UserUid = notification.UserUid,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            RelatedEntityUid = notification.RelatedEntityUid,
            RelatedEntityType = notification.RelatedEntityType
        };

        _context.Notifications.Add(entity);
        await _context.SaveChangesAsync();

        return MapToBo(entity);
    }

    public async Task<NotificationBo> UpdateAsync(NotificationBo notification)
    {
        var entity = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Uid == notification.Uid);

        if (entity == null)
            throw new InvalidOperationException($"Notification with UID {notification.Uid} not found");

        entity.Title = notification.Title;
        entity.Message = notification.Message;
        entity.Type = notification.Type;
        entity.IsRead = notification.IsRead;
        entity.RelatedEntityUid = notification.RelatedEntityUid;
        entity.RelatedEntityType = notification.RelatedEntityType;

        await _context.SaveChangesAsync();

        return MapToBo(entity);
    }

    public async Task DeleteAsync(Guid uid)
    {
        var entity = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Uid == uid);

        if (entity != null)
        {
            _context.Notifications.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAsReadAsync(Guid uid)
    {
        var entity = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Uid == uid);

        if (entity != null)
        {
            entity.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(Guid userUid)
    {
        var entities = await _context.Notifications
            .Where(n => n.UserUid == userUid && !n.IsRead)
            .ToListAsync();

        foreach (var entity in entities)
        {
            entity.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userUid)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserUid == userUid && !n.IsRead);
    }

    private static NotificationBo MapToBo(Entities.Notification entity)
    {
        return new NotificationBo(
            entity.Uid,
            entity.UserUid,
            entity.Title,
            entity.Message,
            entity.Type,
            entity.IsRead,
            entity.CreatedAt,
            entity.RelatedEntityUid,
            entity.RelatedEntityType
        );
    }
}
