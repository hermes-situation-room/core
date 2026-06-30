namespace Hermes.SituationRoom.Data.Repositories;

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

        var memberUids = newChatBo.MemberUids
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (memberUids.Count < 2)
        {
            throw new ValidationBusinessException("MemberUids", "A chat must have at least two distinct members.");
        }

        var isGroup = newChatBo.IsGroup || memberUids.Count > 2;

        // 1:1 chats stay unique per user pair.
        if (!isGroup && memberUids.Count == 2)
        {
            var existingChat = await FindChatByUserPairAsync(memberUids[0], memberUids[1]);
            if (existingChat.HasValue)
            {
                throw new DuplicateResourceException("Chat",
                    $"{memberUids[0]}-{memberUids[1]}",
                    "A chat between these users already exists."
                );
            }
        }

        var chat = new Chat
        {
            Uid = Guid.NewGuid(),
            // Keep the legacy 1:1 columns populated for 2-member chats (display fallback / rollback);
            // for larger groups they stay empty and membership lives entirely in ChatMember.
            User1Uid = memberUids.Count == 2 ? memberUids[0] : Guid.Empty,
            User2Uid = memberUids.Count == 2 ? memberUids[1] : Guid.Empty,
            Name = newChatBo.Name,
            IsGroup = isGroup,
        };

        context.Chats.Add(chat);

        foreach (var userUid in memberUids)
        {
            context.ChatMembers.Add(new ChatMember
            {
                Uid = Guid.NewGuid(),
                ChatUid = chat.Uid,
                UserUid = userUid,
                JoinedAt = DateTime.UtcNow,
            });
        }

        await context.SaveChangesAsync();

        return chat.Uid;
    }

    public async Task<ChatBo> GetChatAsync(Guid chatId)
    {
        var chat = await context.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Uid == chatId)
            ?? throw new ResourceNotFoundException("Chat", chatId.ToString());

        var memberUids = await GetMemberUidsAsync(chatId);
        return MapToChatBo(chat, memberUids);
    }

    public async Task<ChatBo> GetChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        var chatId = await FindChatByUserPairAsync(user1Id, user2Id)
            ?? throw new ResourceNotFoundException("Chat", $"User1: {user1Id}, User2: {user2Id}");

        return await GetChatAsync(chatId);
    }

    public async Task<Guid?> FindChatByUserPairAsync(Guid user1Id, Guid user2Id)
    {
        // A 1:1 chat is a non-group chat whose member set is exactly {user1Id, user2Id}.
        var candidateChatIds = context.ChatMembers
            .AsNoTracking()
            .Where(m => m.UserUid == user1Id)
            .Select(m => m.ChatUid)
            .Intersect(context.ChatMembers
                .AsNoTracking()
                .Where(m => m.UserUid == user2Id)
                .Select(m => m.ChatUid));

        return await context.Chats
            .AsNoTracking()
            .Where(c => !c.IsGroup
                        && candidateChatIds.Contains(c.Uid)
                        && context.ChatMembers.Count(m => m.ChatUid == c.Uid) == 2)
            .Select(c => (Guid?)c.Uid)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ChatBo>> GetChatsByUserAsync(Guid userId)
    {
        var chatIds = await context.ChatMembers
            .AsNoTracking()
            .Where(m => m.UserUid == userId)
            .Select(m => m.ChatUid)
            .ToListAsync();

        var chats = await context.Chats
            .AsNoTracking()
            .Where(c => chatIds.Contains(c.Uid))
            .ToListAsync();

        var membersByChat = await context.ChatMembers
            .AsNoTracking()
            .Where(m => chatIds.Contains(m.ChatUid))
            .ToListAsync();

        return chats
            .Select(chat => MapToChatBo(chat, membersByChat
                .Where(m => m.ChatUid == chat.Uid)
                .Select(m => m.UserUid)
                .ToList()))
            .ToList();
    }

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

    private async Task<IReadOnlyList<Guid>> GetMemberUidsAsync(Guid chatId) => await context.ChatMembers
        .AsNoTracking()
        .Where(m => m.ChatUid == chatId)
        .OrderBy(m => m.JoinedAt)
        .Select(m => m.UserUid)
        .ToListAsync();

    private static ChatBo MapToChatBo(Chat chat, IReadOnlyList<Guid> memberUids)
    {
        // Preserve the 1:1 display path: expose User1/User2 even if they were never
        // persisted (large legacy chats keep the columns, new groups don't).
        var user1 = chat.User1Uid != Guid.Empty
            ? chat.User1Uid
            : memberUids.Count > 0 ? memberUids[0] : Guid.Empty;
        var user2 = chat.User2Uid != Guid.Empty
            ? chat.User2Uid
            : memberUids.Count > 1 ? memberUids[1] : Guid.Empty;

        return new ChatBo(chat.Uid, user1, user2, memberUids, chat.Name, chat.IsGroup);
    }
}
