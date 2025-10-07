namespace Hermes.SituationRoom.Data.Repositories;

using System.Linq.Expressions;
using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Microsoft.EntityFrameworkCore;

public class ChatRepository(IHermessituationRoomContext context) : IChatRepository
{
    public async Task<Guid> AddAsync(ChatBo newChatBo)
    {
        var chat = CreateChat(newChatBo);

        context.Chats.Add(chat);

        await context.SaveChangesAsync();

        return chat.Uid;
    }

    public async Task<ChatBo> GetChatAsync(Guid chatId)
    {
        var chat = await context.Chats
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.Uid == chatId)
                                     ?? throw new KeyNotFoundException($"Could not find Chat with the Id: {chatId}");

        return MapToChatBo(chat);
    }


    public async Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        return MapToChatBo(await context.Chats ...)
                       .AsNoTracking()
                       .FirstOrDefaultAsync(c =>
                           (c.User1Uid == user1Id || c.User2Uid == user1Id)
                           && (c.User1Uid == user2Id || c.User2Uid == user2Id)
                       )
                   ?? throw new KeyNotFoundException($"Could not find Chat with User: {user1Id} and User: {user2Id}");

        return MapToChatBo(chat);
    }

    public async Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId)
    {
        var chats = await context.Chats
            .AsNoTracking()
            .Where(c => c.User1Uid == userId || c.User2Uid == userId)
            .ToListAsync();

        return chats.Select(c => MapToChatBo(c)).ToList();
    }

    public async Task DeleteAsync(Guid chatId)
    {
        var chatToDelete = await context.Chats
            .FirstOrDefaultAsync(c => c.Uid == chatId);

        if (chatToDelete != null)
        {
            context.Chats.Remove(chatToDelete);
            await context.SaveChangesAsync();
        }
    }
    
    private static Chat CreateChat(ChatBo newChatBo) => new()
    {
        Uid = Guid.NewGuid(),
        User1Uid = newChatBo.User1Uid,
        User2Uid = newChatBo.User2Uid,
    };
    
    private static ChatBo MapToChatBo(Chat chat) =>
        new()
        {
            Uid = chat.Uid,
            User1Uid = chat.User1Uid,
            User2Uid = chat.User2Uid,
        };
}
