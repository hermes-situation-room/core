#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Hubs;
using Microsoft.AspNetCore.SignalR;

public class MessageService(IMessageRepository messageRepository, IHubContext<ChatHub> chatHub,
            IUserChatReadStatusService userChatStatusService, IChatService chatService) : IMessageService
{
    public async Task<Guid> AddAsync(NewMessageDto newMessageDto)
    {
        var messageBo = new MessageBo(newMessageDto.Content,
            newMessageDto.SenderUid,
            newMessageDto.ChatUid,
            DateTime.UtcNow
        );
        var newMessageGuid = await messageRepository.AddAsync(messageBo);
        
        var chat = await chatService.GetChatAsync(newMessageDto.ChatUid);
        
        await chatHub.Clients.Group(newMessageDto.ChatUid.ToString())
            .SendAsync("ReceiveMessage", messageBo with { Uid = newMessageGuid, });

        var receiverId = newMessageDto.SenderUid == chat.User1Uid ? chat.User2Uid : chat.User1Uid;
        var countNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId, chat.Uid);

        await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewUnreadChatMessage", chat.Uid, countNewMessages);

        var totalCountNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId);
        await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewTotalUnreadChatMessage", totalCountNewMessages);

        await userChatStatusService.UpdateReadStatusAsync(newMessageDto.SenderUid, newMessageDto.ChatUid);

        return newMessageGuid;
    }

    public Task<MessageBo> GetMessageAsync(Guid messageId) => messageRepository.GetMessageAsync(messageId);

    public Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId) =>
        messageRepository.GetMessagesByChatAsync(chatId);

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
}
