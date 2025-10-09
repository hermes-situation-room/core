#nullable enable
namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Data.Interface;
using Interfaces;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public class MessageService(IMessageRepository messageRepository) : IMessageService
{
    public Task<Guid> AddAsync(NewMessageDto newMessageDto) =>
        messageRepository.AddAsync(
            new (newMessageDto.Content,
                newMessageDto.SenderUid,
                newMessageDto.ChatUid,
                DateTime.Now));

    public Task<MessageBo> GetMessageAsync(Guid messageId) => 
        messageRepository.GetMessageAsync(messageId);

    public Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId) => 
        messageRepository.GetMessagesByChatAsync(chatId);

    public Task UpdateAsync(Guid messageId, string newContent) => 
        messageRepository.UpdateAsync(messageId, newContent);

    public Task DeleteAsync(Guid messageId) => 
        messageRepository.DeleteAsync(messageId);
}
