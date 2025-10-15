#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using Data.Interface;
using Hubs;
using Interfaces;
using Microsoft.AspNetCore.SignalR;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Shared.Exceptions;

public class MessageService(IMessageRepository messageRepository, IHubContext<ChatHub> chatHub) : IMessageService
{
    public async Task<Guid> AddAsync(NewMessageDto newMessageDto)
    {
        ArgumentNullException.ThrowIfNull(newMessageDto, nameof(newMessageDto));

        if (newMessageDto.ChatUid == Guid.Empty)
            throw new ArgumentException("Chat ID cannot be empty.", nameof(newMessageDto.ChatUid));

        if (newMessageDto.SenderUid == Guid.Empty)
            throw new ArgumentException("Sender ID cannot be empty.", nameof(newMessageDto.SenderUid));

        if (string.IsNullOrWhiteSpace(newMessageDto.Content))
            throw new BusinessValidationException(nameof(newMessageDto.Content), "Message content is required.");

        if (newMessageDto.Content.Length > 2000)
            throw new BusinessValidationException(nameof(newMessageDto.Content), "Message content cannot exceed 2000 characters.");

        var messageBo = new MessageBo(newMessageDto.Content,
            newMessageDto.SenderUid,
            newMessageDto.ChatUid,
            DateTime.UtcNow
        );
        var newMessageGuid = await messageRepository.AddAsync(messageBo);

        await chatHub.Clients.Group(newMessageDto.ChatUid.ToString())
            .SendAsync("ReceiveMessage", messageBo with { Uid = newMessageGuid, });

        return newMessageGuid;
    }

    public async Task<MessageBo> GetMessageAsync(Guid messageId)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));

        return await messageRepository.GetMessageAsync(messageId);
    }

    public async Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId)
    {
        if (chatId == Guid.Empty)
            throw new ArgumentException("Chat ID cannot be empty.", nameof(chatId));

        return await messageRepository.GetMessagesByChatAsync(chatId);
    }

    public async Task UpdateAsync(Guid messageId, string newContent)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));

        if (string.IsNullOrWhiteSpace(newContent))
            throw new BusinessValidationException(nameof(newContent), "Message content is required.");

        if (newContent.Length > 2000)
            throw new BusinessValidationException(nameof(newContent), "Message content cannot exceed 2000 characters.");

        var messageBo = await messageRepository.UpdateAsync(messageId, newContent);
        await chatHub.Clients.Group(messageBo.ChatUid.ToString()).SendAsync("UpdateMessage", messageBo);
    }

    public async Task DeleteAsync(Guid messageId)
    {
        if (messageId == Guid.Empty)
            throw new ArgumentException("Message ID cannot be empty.", nameof(messageId));

        var message = await messageRepository.GetMessageAsync(messageId);

        await messageRepository.DeleteAsync(messageId);

        await chatHub.Clients.Group(message.ChatUid.ToString()).SendAsync("DeleteMessage", messageId);
    }
}
