namespace Hermes.SituationRoom.Domain.Services;

using AutoMapper;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Data.Interface;
using Interfaces;

public class ChatService(IChatRepository chatRepository, IUserChatReadStatusService userReadStatusService, IMapper mapper) : IChatService
{
    public async Task<Guid> AddAsync(CreateChatRequestDto createChatDto)
    {
        var chatBo = mapper.Map<ChatBo>(createChatDto);

        // 1:1 chats are unique per user pair: return the existing one instead of duplicating.
        if (!chatBo.IsGroup && chatBo.MemberUids.Count == 2)
        {
            var existingChatId = await chatRepository.FindChatByUserPairAsync(chatBo.MemberUids[0], chatBo.MemberUids[1]);
            if (existingChatId.HasValue)
            {
                return existingChatId.Value;
            }
        }

        var newChatId = await chatRepository.AddAsync(chatBo);

        foreach (var memberUid in chatBo.MemberUids)
        {
            await userReadStatusService.CreateReadStatusAsync(memberUid, newChatId);
        }

        return newChatId;
    }

    public async Task<ChatDto> GetChatAsync(Guid chatId)
    {
        var chat = await chatRepository.GetChatAsync(chatId);
        return mapper.Map<ChatDto>(chat);
    }

    public async Task<ChatDto> GetChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        var chat = await chatRepository.GetChatByUserPairAsync(user1Id, user2Id);
        return mapper.Map<ChatDto>(chat);
    }

    public async Task<ChatDto> GetOrCreateChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        var existingChatId = await chatRepository.FindChatByUserPairAsync(user1Id, user2Id);
        
        if (existingChatId.HasValue)
        {
            var existingChat = await chatRepository.GetChatAsync(existingChatId.Value);
            return mapper.Map<ChatDto>(existingChat);
        }

        var newChatBo = new ChatBo(Guid.NewGuid(), user1Id, user2Id);
        var chatId = await chatRepository.AddAsync(newChatBo);

        foreach (var memberUid in newChatBo.MemberUids)
        {
            await userReadStatusService.CreateReadStatusAsync(memberUid, chatId);
        }

        var chat = await chatRepository.GetChatAsync(chatId);
        return mapper.Map<ChatDto>(chat);
    }

    public async Task<IReadOnlyList<ChatDto>> GetChatsByUserAsync(Guid userId)
    {
        var chats = await chatRepository.GetChatsByUserAsync(userId);
        return mapper.Map<IReadOnlyList<ChatDto>>(chats);
    }

    public Task DeleteAsync(Guid chatId) => chatRepository.DeleteAsync(chatId);
}
