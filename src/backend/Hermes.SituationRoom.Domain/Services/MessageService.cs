#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Hubs;
using Microsoft.AspNetCore.SignalR;

public class MessageService(IMessageRepository messageRepository, IHubContext<ChatHub> chatHub) : IMessageService
{
    public async Task<Guid> AddAsync(NewMessageDto newMessageDto)
    {
        var messageBo = new MessageBo(newMessageDto.Content,
            newMessageDto.SenderUid,
            newMessageDto.ChatUid,
            DateTime.UtcNow
        );
        var newMessageGuid = await messageRepository.AddAsync(messageBo);
        
        await chatHub.Clients.Group(newMessageDto.ChatUid.ToString()).SendAsync("ReceiveMessage", messageBo);

        return newMessageGuid;
    }

    public Task<MessageBo> GetMessageAsync(Guid messageId) => 
        messageRepository.GetMessageAsync(messageId);

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
