#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Shared.BusinessObjects;
using Microsoft.EntityFrameworkCore;

public class MessageRepository(IHermessituationRoomContext context) : IMessageRepository
{
    public async Task<Guid> AddAsync(MessageBo newMessageBo)
    {
        var message = CreateMessage(newMessageBo);
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        return message.Uid;
    }

    public async Task<MessageBo> GetMessageAsync(Guid messageId) =>
        MapToMessageBo(await context.Messages
                           .AsNoTracking()
                           .FirstOrDefaultAsync(m => m.Uid == messageId)
                       ?? throw new KeyNotFoundException($"Could not find Message with the Id: {messageId}"));

    public async Task<MessageBo?> GetMessageForUserAsync(Guid messageId, Guid userId)
    {
        var now = DateTime.UtcNow;

        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Uid == messageId);

        // Not found, or its disappear timer has passed: no content for anyone.
        if (message == null || (message.ExpiresAt != null && message.ExpiresAt <= now))
        {
            return null;
        }

        // The requester must be a member of the message's chat. Otherwise an outsider who
        // guesses a messageId could read content or burn a recipient's single view-once reveal.
        if (!await IsChatMemberAsync(message.ChatUid, userId))
        {
            return null;
        }

        var bo = MapToMessageBo(message);

        // Non-view-once messages (normal + disappearing) are returned as-is.
        if (!message.IsViewOnce)
        {
            return bo;
        }

        // The sender always sees their own view-once content and never consumes it.
        if (message.SenderUid == userId)
        {
            return bo;
        }

        // Recipient path: this GET is the consume/reveal. Try to atomically record the view.
        // If the row already exists (already consumed), no content is returned.
        var consumed = await TryConsumeViewOnceAsync(messageId, userId, now);
        return consumed ? bo : null;
    }

    /// <summary>
    /// True if the user is a member of the given chat (n:m membership via ChatMember).
    /// </summary>
    private Task<bool> IsChatMemberAsync(Guid chatUid, Guid userId) =>
        context.ChatMembers
            .AsNoTracking()
            .AnyAsync(cm => cm.ChatUid == chatUid && cm.UserUid == userId);

    /// <summary>
    /// Atomically records a MessageView for (messageId, userId). Returns true if THIS call inserted
    /// the row (first reveal -> content may be returned once); false if it was already consumed.
    /// Relies on the unique (MessageUid, UserUid) constraint to make concurrent first-reveals safe.
    /// </summary>
    private async Task<bool> TryConsumeViewOnceAsync(Guid messageId, Guid userId, DateTime now)
    {
        var alreadyViewed = await context.MessageViews
            .AsNoTracking()
            .AnyAsync(v => v.MessageUid == messageId && v.UserUid == userId);

        if (alreadyViewed)
        {
            return false;
        }

        context.MessageViews.Add(new MessageView
        {
            Uid = Guid.NewGuid(),
            MessageUid = messageId,
            UserUid = userId,
            ViewedAt = now
        });

        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // A concurrent request inserted the view first; this fetch loses the race and is blocked.
            return false;
        }
    }

    public async Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId)
    {
        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.ChatUid == chatId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return messages.Select(MapToMessageBo).ToList();
    }

    public async Task<IReadOnlyList<MessageBo>> GetMessagesByChatForUserAsync(Guid chatId, Guid userId)
    {
        var now = DateTime.UtcNow;

        // Cleanup-on-read: physically purge messages whose disappear timer has passed.
        await PurgeExpiredInChatAsync(chatId, now);

        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.ChatUid == chatId && (m.ExpiresAt == null || m.ExpiresAt > now))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        // Which view-once messages has this user already consumed?
        var messageIds = messages.Where(m => m.IsViewOnce).Select(m => m.Uid).ToList();
        var consumedIds = messageIds.Count == 0
            ? new HashSet<Guid>()
            : (await context.MessageViews
                    .AsNoTracking()
                    .Where(v => v.UserUid == userId && messageIds.Contains(v.MessageUid))
                    .Select(v => v.MessageUid)
                    .ToListAsync())
                .ToHashSet();

        return messages.Select(message =>
        {
            var bo = MapToMessageBo(message);
            // A view-once message the sender never sees consumed; recipients who have
            // already viewed it get a blanked, consumed marker instead of content.
            if (message.IsViewOnce && message.SenderUid != userId && consumedIds.Contains(message.Uid))
            {
                return bo with { Content = string.Empty, IsConsumed = true };
            }
            return bo;
        }).ToList();
    }

    public async Task<MessageBo> UpdateAsync(Guid messageId, string newContent)
    {
        var messageToUpdate = await context.Messages
                                  .AsTracking()
                                  .FirstOrDefaultAsync(m => m.Uid == messageId)
                              ?? throw new KeyNotFoundException($"Could not find Message with the Id: {messageId}");

        messageToUpdate.Content = newContent;
        await context.SaveChangesAsync();
        return MapToMessageBo(messageToUpdate);
    }

    public async Task DeleteAsync(Guid messageId)
    {
        var messageToDelete = await context.Messages
            .FirstOrDefaultAsync(m => m.Uid == messageId);

        if (messageToDelete != null)
        {
            context.Messages.Remove(messageToDelete);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> MarkViewedAsync(Guid messageId, Guid userId)
    {
        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Uid == messageId);

        if (message == null || !message.IsViewOnce)
        {
            return false;
        }

        // Only a member of the message's chat may consume it.
        if (!await IsChatMemberAsync(message.ChatUid, userId))
        {
            return false;
        }

        var alreadyViewed = await context.MessageViews
            .AsNoTracking()
            .AnyAsync(v => v.MessageUid == messageId && v.UserUid == userId);

        if (alreadyViewed)
        {
            return true;
        }

        context.MessageViews.Add(new MessageView
        {
            Uid = Guid.NewGuid(),
            MessageUid = messageId,
            UserUid = userId,
            ViewedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<(Guid ChatUid, Guid MessageUid)>> PurgeExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await context.Messages
            .Where(m => m.ExpiresAt != null && m.ExpiresAt <= now)
            .ToListAsync();

        if (expired.Count == 0)
        {
            return new List<(Guid, Guid)>();
        }

        var purged = expired.Select(m => (m.ChatUid, m.Uid)).ToList();
        context.Messages.RemoveRange(expired);
        await context.SaveChangesAsync();
        return purged;
    }

    private async Task PurgeExpiredInChatAsync(Guid chatId, DateTime now)
    {
        var expired = await context.Messages
            .Where(m => m.ChatUid == chatId && m.ExpiresAt != null && m.ExpiresAt <= now)
            .ToListAsync();

        if (expired.Count > 0)
        {
            context.Messages.RemoveRange(expired);
            await context.SaveChangesAsync();
        }
    }

    private static Message CreateMessage(MessageBo messageBo) => new()
    {
        Uid = Guid.NewGuid(),
        Timestamp = messageBo.Timestamp,
        Content = messageBo.Content,
        SenderUid = messageBo.SenderUid,
        ChatUid = messageBo.ChatUid,
        ExpiresAt = messageBo.ExpiresAt,
        IsViewOnce = messageBo.IsViewOnce
    };

    private static MessageBo MapToMessageBo(Message message) =>
        new(message.Content, message.SenderUid, message.ChatUid, message.Timestamp)
        {
            Uid = message.Uid,
            ExpiresAt = message.ExpiresAt,
            IsViewOnce = message.IsViewOnce
        };
}
