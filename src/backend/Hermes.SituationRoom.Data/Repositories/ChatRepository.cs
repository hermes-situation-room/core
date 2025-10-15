namespace Hermes.SituationRoom.Data.Repositories;

using System.Linq.Expressions;
using Hermes.SituationRoom.Data.Entities;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Hermes.SituationRoom.Shared.Exceptions;

public class ChatRepository(IHermessituationRoomContext context) : IChatRepository
{
    public async Task<Guid> AddAsync(ChatBo newChatBo)
    {
        ArgumentNullException.ThrowIfNull(newChatBo);

        if (newChatBo.User1Uid == Guid.Empty || newChatBo.User2Uid == Guid.Empty)
        {
            throw new ValidationBusinessException("UserIds", "Both user IDs must be valid.");
        }

        if (newChatBo.User1Uid == newChatBo.User2Uid)
        {
            throw new ValidationBusinessException("UserIds", "Cannot create a chat with yourself.");
        }

        var existingChat = await FindChatByUserPairAsync(newChatBo.User1Uid, newChatBo.User2Uid);
        if (existingChat.HasValue)
        {
            throw new DuplicateResourceException("Chat",
                $"{newChatBo.User1Uid}-{newChatBo.User2Uid}",
                "A chat between these users already exists."
            );
        }

        var chat = CreateChat(newChatBo);

        context.Chats.Add(chat);

        await context.SaveChangesAsync();

        return chat.Uid;
    }

    public async Task<ChatBo> GetChatAsync(Guid chatId) => MapToChatBo(await context.Chats
                                                                           .AsNoTracking()
                                                                           .FirstOrDefaultAsync(c => c.Uid == chatId)
                                                                       ?? throw new ResourceNotFoundException("Chat",
                                                                           chatId.ToString()
                                                                       )
    );

    public async Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id) => MapToChatBo(await context.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                (c.User1Uid == user1Id || c.User2Uid == user1Id)
                && (c.User1Uid == user2Id || c.User2Uid == user2Id)
            )
        ?? throw new ResourceNotFoundException("Chat", $"User1: {user1Id}, User2: {user2Id}")
    );

    public async Task<Guid?> FindChatByUserPairAsync(Guid user1Id, Guid user2Id) => await context.Chats
        .AsNoTracking()
        .Where(c =>
            (c.User1Uid == user1Id || c.User2Uid == user1Id)
            && (c.User1Uid == user2Id || c.User2Uid == user2Id)
        )
        .Select(c => (Guid?)c.Uid)
        .FirstOrDefaultAsync();

    public async Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId) => await context.Chats
        .AsNoTracking()
        .Where(c => c.User1Uid == userId || c.User2Uid == userId)
        .Select(c => new ChatBo(c.Uid, c.User1Uid, c.User2Uid))
        .ToListAsync();

    public async Task DeleteAsync(Guid chatId)
    {
        var chatToDelete = await context.Chats
            .FirstOrDefaultAsync(c => c.Uid == chatId);

        if (chatToDelete == null)
        {
            throw new ResourceNotFoundException("Chat", chatId.ToString());
        }

        context.Chats.Remove(chatToDelete);
        await context.SaveChangesAsync();
    }

    private static Chat CreateChat(ChatBo newChatBo) => new()
    {
        Uid = Guid.NewGuid(), User1Uid = newChatBo.User1Uid, User2Uid = newChatBo.User2Uid,
    };

    private static ChatBo MapToChatBo(Chat chat) => new(chat.Uid, chat.User1Uid, chat.User2Uid);
}
