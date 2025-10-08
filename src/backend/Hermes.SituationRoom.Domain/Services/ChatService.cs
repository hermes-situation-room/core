namespace Hermes.SituationRoom.Domain.Services;

using Shared.BusinessObjects;
using Data.Interface;
using Interfaces;

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<Guid> AddAsync(ChatBo newChatBo)
    {
        var chatId = await chatRepository.FindChatByUserPairAsync(newChatBo.User1Uid, newChatBo.User2Uid);
        
        if (chatId.HasValue)
        {
            return chatId.Value;
        }

        return await chatRepository.AddAsync(newChatBo);
    }

    public Task<ChatBo> GetChatAsync(Guid chatId) => chatRepository.GetChatAsync(chatId);

    public Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id) =>
        chatRepository.GetChatByUserPairAsync(user1Id, user2Id);

    public Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId) => chatRepository.GetChatsByUserAsync(userId);

    public Task DeleteAsync(Guid chatId) => chatRepository.DeleteAsync(chatId);
}
