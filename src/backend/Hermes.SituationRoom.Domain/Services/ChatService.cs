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
        var chatId = await chatRepository.FindChatByUserPairAsync(createChatDto.User1Uid, createChatDto.User2Uid);
        
        if (chatId.HasValue)
        {
            return chatId.Value;
        }

        var chatBo = mapper.Map<ChatBo>(createChatDto);
        var newChatId = await chatRepository.AddAsync(chatBo);

        await userReadStatusService.CreateReadStatusAsync(createChatDto.User1Uid, newChatId);
        await userReadStatusService.CreateReadStatusAsync(createChatDto.User2Uid, newChatId);

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

        await userReadStatusService.CreateReadStatusAsync(newChatBo.User1Uid, chatId);
        await userReadStatusService.CreateReadStatusAsync(newChatBo.User2Uid, chatId);
        
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
