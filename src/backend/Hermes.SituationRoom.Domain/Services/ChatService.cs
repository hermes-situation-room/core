namespace Hermes.SituationRoom.Domain.Services;

using Shared.BusinessObjects;
using Data.Interface;
using Interfaces;
using Shared.Exceptions;

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<Guid> AddAsync(ChatBo newChatBo)
    {
        ArgumentNullException.ThrowIfNull(newChatBo, nameof(newChatBo));

        if (newChatBo.User1Uid == Guid.Empty)
            throw new ArgumentException("User 1 ID cannot be empty.", nameof(newChatBo.User1Uid));

        if (newChatBo.User2Uid == Guid.Empty)
            throw new ArgumentException("User 2 ID cannot be empty.", nameof(newChatBo.User2Uid));

        if (newChatBo.User1Uid == newChatBo.User2Uid)
            throw new BusinessValidationException("Chat", "Cannot create a chat with yourself.");

        var chatId = await chatRepository.FindChatByUserPairAsync(newChatBo.User1Uid, newChatBo.User2Uid);
        
        if (chatId.HasValue)
        {
            return chatId.Value;
        }

        return await chatRepository.AddAsync(newChatBo);
    }

    public async Task<ChatBo> GetChatAsync(Guid chatId)
    {
        if (chatId == Guid.Empty)
            throw new ArgumentException("Chat ID cannot be empty.", nameof(chatId));

        return await chatRepository.GetChatAsync(chatId);
    }

    public async Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        if (user1Id == Guid.Empty)
            throw new ArgumentException("User 1 ID cannot be empty.", nameof(user1Id));

        if (user2Id == Guid.Empty)
            throw new ArgumentException("User 2 ID cannot be empty.", nameof(user2Id));

        return await chatRepository.GetChatByUserPairAsync(user1Id, user2Id);
    }

    public async Task<ChatBo> GetOrCreateChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        if (user1Id == Guid.Empty)
            throw new ArgumentException("User 1 ID cannot be empty.", nameof(user1Id));

        if (user2Id == Guid.Empty)
            throw new ArgumentException("User 2 ID cannot be empty.", nameof(user2Id));

        if (user1Id == user2Id)
            throw new BusinessValidationException("Chat", "Cannot create a chat with yourself.");

        var existingChatId = await chatRepository.FindChatByUserPairAsync(user1Id, user2Id);
        
        if (existingChatId.HasValue)
        {
            return await chatRepository.GetChatAsync(existingChatId.Value);
        }

        var newChatBo = new ChatBo(user1Id, user2Id);
        var chatId = await chatRepository.AddAsync(newChatBo);
        return await chatRepository.GetChatAsync(chatId);
    }

    public async Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return await chatRepository.GetChatsByUserAsync(userId);
    }

    public async Task DeleteAsync(Guid chatId)
    {
        if (chatId == Guid.Empty)
            throw new ArgumentException("Chat ID cannot be empty.", nameof(chatId));

        await chatRepository.DeleteAsync(chatId);
    }
}
