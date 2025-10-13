#nullable enable
namespace Hermes.SituationRoom.Data.Repositories;

using Entities;
using Interface;
using Shared.BusinessObjects;
using Microsoft.EntityFrameworkCore;

public class UserChatReadStatusRepository(IHermessituationRoomContext context) : IUserChatReadStatusRepository
{
      public async Task<Guid> AddAsync(UserChatReadStatusBo newReadStatusBo)
    {
        var readStatus = CreateReadStatus(newReadStatusBo);
        context.UserChatReadStatuses.Add(readStatus);
        await context.SaveChangesAsync();
        return readStatus.UserChatReadStatusId;
    }

    public async Task<UserChatReadStatusBo> GetReadStatusAsync(Guid readStatusId) =>
        MapToBo(await context.UserChatReadStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(rs => rs.UserChatReadStatusId == readStatusId)
            ?? throw new KeyNotFoundException($"Could not find ChatReadStatus with Id: {readStatusId}")
        );

    public async Task<UserChatReadStatusBo> GetReadStatusByUserAndChatAsync(Guid userId, Guid chatId) =>
        MapToBo(await context.UserChatReadStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(rs => rs.UserId == userId && rs.ChatId == chatId)
            ?? throw new KeyNotFoundException($"Could not find ChatReadStatus for User: {userId} and Chat: {chatId}")
        );

    public async Task<UserChatReadStatusBo> UpdateAsync(Guid readStatusId)
    {
        var readStatusToUpdate = await context.UserChatReadStatuses
            .AsTracking()
            .FirstOrDefaultAsync(rs => rs.UserChatReadStatusId == readStatusId)
            ?? throw new KeyNotFoundException($"Could not find ChatReadStatus with Id: {readStatusId}");

        readStatusToUpdate.ReadTime = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return MapToBo(readStatusToUpdate);
    }

    public async Task<UserChatReadStatusBo> UpdateByUserAndChatAsync(Guid userId, Guid chatId)
    {
        var readStatusToUpdate = await context.UserChatReadStatuses
            .AsTracking()
            .FirstOrDefaultAsync(rs => rs.UserId == userId && rs.ChatId == chatId)
            ?? throw new KeyNotFoundException($"Could not find ChatReadStatus for User: {userId} and Chat: {chatId}");

        readStatusToUpdate.ReadTime = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return MapToBo(readStatusToUpdate);
    }

    public async Task DeleteAsync(Guid readStatusId)
    {
        var readStatusToDelete = new UserChatReadStatus { UserChatReadStatusId = readStatusId };
        context.UserChatReadStatuses.Remove(readStatusToDelete);
        await context.SaveChangesAsync();
    }

    private static UserChatReadStatus CreateReadStatus(UserChatReadStatusBo readStatusBo) => new()
    {
        UserChatReadStatusId = Guid.NewGuid(),
        UserId = readStatusBo.UserId,
        ChatId = readStatusBo.ChatId,
        ReadTime = readStatusBo.ReadTime
    };

    private static UserChatReadStatusBo MapToBo(UserChatReadStatus readStatus) =>
        new(readStatus.UserId, readStatus.ChatId, readStatus.ReadTime) {Uid = readStatus.UserChatReadStatusId};

}