#nullable enable
namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IMessageService
{
    Task<Guid> AddAsync(CreateMessageRequestDto createMessageDto);

    Task<MessageDto> GetMessageAsync(Guid messageId);

    Task<IReadOnlyList<MessageDto>> GetMessagesByChatAsync(Guid chatId);

    Task UpdateAsync(Guid messageId, string newContent);
    
    Task DeleteAsync(Guid messageId);
}
