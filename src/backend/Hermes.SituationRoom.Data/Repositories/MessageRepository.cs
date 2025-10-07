#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Shared.BusinessObjects;
using Microsoft.EntityFrameworkCore;

public class MessageRepository(IHermessituationRoomContext context) : IMessageRepository
{
    public async Task<Guid> AddAsync(MessageBo newMessageBo)
    {
        var message = CreateMessage(newMessageBo);
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        return message.Uid;
    }

    public async Task<MessageBo> GetMessageAsync(Guid messageId)
    {
        var message = await context.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Uid == messageId)
            ?? throw new KeyNotFoundException($"Could not find Message with the Id: {messageId}");

        return MapToMessageBo(message);
    }

    public async Task<IReadOnlyList<MessageBo>> GetMessagesByChatAsync(Guid chatId)
    {
        var messages = await context.Messages
            .AsNoTracking()
            .Where(m => m.ChatUid == chatId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return messages.Select(MapToMessageBo).ToList();
    }

    public async Task UpdateAsync(Guid messageId, string newContent)
    {
        var messageToUpdate = await context.Messages
            .FirstOrDefaultAsync(m => m.Uid == messageId)
            ?? throw new KeyNotFoundException($"Could not find Message with the Id: {messageId}");
        
        messageToUpdate.Content = newContent;
        context.Messages.Update(messageToUpdate);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(Guid messageId)
    {
        var messageToDelete = await context.Messages
            .FirstOrDefaultAsync(m => m.Uid == messageId);

        if (messageToDelete != null)
        {
            context.Messages.Remove(messageToDelete);
            await context.SaveChangesAsync();
        }
    }

    private static Message CreateMessage(MessageBo messageBo) => new()
    {
        Uid = Guid.NewGuid(),
        Timestamp = messageBo.Timestamp, 
        Content = messageBo.Content,
        SenderUid = messageBo.SenderUid,
        ChatUid = messageBo.ChatUid
    };

    private static MessageBo MapToMessageBo(Message message) =>
        new(message.Content, message.SenderUid, message.ChatUid, message.Timestamp)
        {
            Uid = message.Uid,
        };
}