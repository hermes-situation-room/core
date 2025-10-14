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

    public async Task<ChatBo> GetOrCreateChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        var existingChatId = await chatRepository.FindChatByUserPairAsync(user1Id, user2Id);
        
        if (existingChatId.HasValue)
        {
            return await chatRepository.GetChatAsync(existingChatId.Value);
        }

        var newChatBo = new ChatBo(user1Id, user2Id);
        var chatId = await chatRepository.AddAsync(newChatBo);
        return await chatRepository.GetChatAsync(chatId);
    }

    public Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId) => chatRepository.GetChatsByUserAsync(userId);

    public Task DeleteAsync(Guid chatId) => chatRepository.DeleteAsync(chatId);
}
