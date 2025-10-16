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
            IUserChatReadStatusService userChatStatusService, IChatService chatService, IMapper mapper) : IMessageService
{
    public async Task<Guid> AddAsync(CreateMessageRequestDto createMessageDto)
    {
        var messageBo = mapper.Map<MessageBo>(createMessageDto);
        messageBo = messageBo with { Timestamp = DateTime.UtcNow };
        
        var newMessageGuid = await messageRepository.AddAsync(messageBo);
        
        var chat = await chatService.GetChatAsync(createMessageDto.ChatUid);
        
        await chatHub.Clients.Group(createMessageDto.ChatUid.ToString())
            .SendAsync("ReceiveMessage", messageBo with { Uid = newMessageGuid, });

        var receiverId = createMessageDto.SenderUid == chat.User1Uid ? chat.User2Uid : chat.User1Uid;
        var countNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId, chat.Uid);

        await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewUnreadChatMessage", chat.Uid, countNewMessages);

        var totalCountNewMessages = await userChatStatusService.GetUnreadMessageCountAsync(receiverId);
        await chatHub.Clients.Group(receiverId.ToString()).SendAsync("NewTotalUnreadChatMessage", totalCountNewMessages);

        await userChatStatusService.UpdateReadStatusAsync(createMessageDto.SenderUid, createMessageDto.ChatUid);

        return newMessageGuid;
    }

    public async Task<MessageDto> GetMessageAsync(Guid messageId)
    {
        var message = await messageRepository.GetMessageAsync(messageId);
        return mapper.Map<MessageDto>(message);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesByChatAsync(Guid chatId)
    {
        var messages = await messageRepository.GetMessagesByChatAsync(chatId);
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
}
