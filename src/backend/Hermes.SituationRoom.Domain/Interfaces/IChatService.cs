namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IChatService
{
    Task<Guid> AddAsync(CreateChatRequestDto createChatDto);

    Task<ChatDto> GetChatAsync(Guid chatId);

    Task<ChatDto> GetChatByUserPairAsync(Guid user1Id, Guid user2Id);

    Task<ChatDto> GetOrCreateChatByUserPairAsync(Guid user1Id, Guid user2Id);

    Task<IReadOnlyList<ChatDto>> GetChatsByUserAsync(Guid userId);
    
    Task DeleteAsync(Guid chatId);
}
