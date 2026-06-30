#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Hermes.SituationRoom.Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Hubs;
using Microsoft.AspNetCore.SignalR;

public class MessageService(IMessageRepository messageRepository, IHubContext<ChatHub> chatHub,
            IUserChatReadStatusService userChatStatusService, IChatService chatService, IMapper mapper, INotificationService notificationService, IUserService userService, IAttachmentService attachmentService) : IMessageService
{
    public async Task<Guid> AddAsync(CreateMessageRequestDto createMessageDto)
    {
        var messageBo = mapper.Map<MessageBo>(createMessageDto);
        messageBo = messageBo with { Timestamp = DateTime.UtcNow };

        var newMessageGuid = await messageRepository.AddAsync(messageBo);

        // Link any uploaded attachments to the new message (sender-scoped) so chat
        // members are authorized to download them.
        if (createMessageDto.AttachmentIds is { Count: > 0 })
        {
            await attachmentService.LinkToMessageAsync(createMessageDto.AttachmentIds, createMessageDto.SenderUid, newMessageGuid);
        }

        var chat = await chatService.GetChatAsync(createMessageDto.ChatUid);

        // View-once messages must never leak their plaintext over the real-time fan-out: recipients
        // get a locked placeholder (no content) and must reveal via the consume GET. Normal and
        // disappearing (non-view-once) messages keep broadcasting their content as before.
        var broadcastBo = messageBo with { Uid = newMessageGuid };
        if (broadcastBo.IsViewOnce)
        {
            broadcastBo = broadcastBo with { Content = string.Empty };
        }

        await chatHub.Clients.Group(createMessageDto.ChatUid.ToString())
            .SendAsync("ReceiveMessage", broadcastBo);

        // Fan out to every chat member except the sender (1:1 chats have exactly one receiver).
        var receiverIds = chat.MemberUids
            .Where(id => id != createMessageDto.SenderUid)
            .Distinct()
            .ToList();

        foreach (var receiverId in receiverIds)
        {
            var countNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId, chat.Uid);
            await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewUnreadChatMessage", chat.Uid, countNewMessages);

            var totalCountNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId);
            await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewTotalUnreadChatMessage", totalCountNewMessages);
        }

        // Tell every participant's chat list to refresh (new last-message / ordering / unread count)
        try
        {
            foreach (var receiverId in receiverIds)
            {
                await chatHub.Clients.Group(receiverId.ToString()).SendAsync("ChatListUpdated", chat.Uid);
            }
            await chatHub.Clients.Group(createMessageDto.SenderUid.ToString()).SendAsync("ChatListUpdated", chat.Uid);
        }
        catch (Exception)
        {
            // Don't fail message creation if SignalR fails
        }

        await userChatStatusService.UpdateReadStatusAsync(createMessageDto.SenderUid, createMessageDto.ChatUid);

        // Create a notification for each receiver
        try
        {
            foreach (var receiverId in receiverIds)
            {
                var senderProfile = await userService.GetUserProfileAsync(createMessageDto.SenderUid, receiverId);
                var senderName = GetDisplayName(senderProfile);
                await notificationService.CreateMessageNotificationAsync(receiverId, senderName, chat.Uid);
            }
        }
        catch (Exception)
        {
            // Don't fail message creation if notification fails
        }

        return newMessageGuid;
    }

    public async Task<MessageDto> GetMessageAsync(Guid messageId, Guid requestingUserId)
    {
        var message = await messageRepository.GetMessageForUserAsync(messageId, requestingUserId)
            ?? throw new KeyNotFoundException($"Could not find Message with the Id: {messageId}");
        return mapper.Map<MessageDto>(message);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesByChatAsync(Guid chatId, Guid requestingUserId)
    {
        var messages = await messageRepository.GetMessagesByChatForUserAsync(chatId, requestingUserId);
        return mapper.Map<IReadOnlyList<MessageDto>>(messages);
    }

    public async Task UpdateAsync(Guid messageId, string newContent)
    {
        var messageBo = await messageRepository.UpdateAsync(messageId, newContent);
        await chatHub.Clients.Group(messageBo.ChatUid.ToString()).SendAsync("UpdateMessage", messageBo);
    }

    public async Task DeleteAsync(Guid messageId)
    {
        var message = await messageRepository.GetMessageAsync(messageId);

        await messageRepository.DeleteAsync(messageId);

        await chatHub.Clients.Group(message.ChatUid.ToString()).SendAsync("DeleteMessage", messageId);
    }

    public async Task<bool> MarkViewedAsync(Guid messageId, Guid userId)
    {
        var message = await messageRepository.GetMessageAsync(messageId);

        var marked = await messageRepository.MarkViewedAsync(messageId, userId);
        if (!marked)
        {
            return false;
        }

        // The content is now consumed for this recipient: tell only this user's connections to
        // drop it live. Other members keep their own view-once state until they reveal it.
        await chatHub.Clients.Group(userId.ToString()).SendAsync("MessageExpired", message.ChatUid, messageId);
        return true;
    }

    public async Task<int> PurgeExpiredAsync()
    {
        var purged = await messageRepository.PurgeExpiredAsync();

        foreach (var (chatUid, messageUid) in purged)
        {
            // Broadcast to the chat group so every open chat detail drops the message live.
            await chatHub.Clients.Group(chatUid.ToString()).SendAsync("MessageExpired", chatUid, messageUid);
        }

        return purged.Count;
    }

    private static string GetDisplayName(UserProfileDto? profile)
    {
        if (profile == null) return "Someone";
        
        if (!string.IsNullOrEmpty(profile.FirstName) && !string.IsNullOrEmpty(profile.LastName))
            return $"{profile.FirstName} {profile.LastName}";
        
        if (!string.IsNullOrEmpty(profile.UserName))
            return profile.UserName;
        
        if (!string.IsNullOrEmpty(profile.FirstName))
            return profile.FirstName;
        
        return "Someone";
    }
}
