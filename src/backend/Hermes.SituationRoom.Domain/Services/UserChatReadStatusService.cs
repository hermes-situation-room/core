namespace Hermes.SituationRoom.Domain.Services;

using System;
using System.Threading.Tasks;
using Hermes.SituationRoom.Data.Interface;
using Hermes.SituationRoom.Domain.Hubs;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Microsoft.AspNetCore.SignalR;

public class UserChatReadStatusService(IUserChatReadStatusRepository userChatReadStatusRepository, IHubContext<ChatHub> chatHub ): IUserChatReadStatusService
{
    public Task<Guid> CreateReadStatusAsync(Guid userId, Guid chatId)
    {
        var bo = new UserChatReadStatusBo(userId, chatId, DateTime.UtcNow);
        return userChatReadStatusRepository.AddAsync(bo);
    }


    public Task<UserChatReadStatusBo> GetReadStatusAsync(Guid uid) =>
        userChatReadStatusRepository.GetReadStatusAsync(uid);

    public Task<UserChatReadStatusBo> GetReadStatusAsync(Guid userId, Guid chatId) =>
        userChatReadStatusRepository.GetReadStatusAsync(userId, chatId);

    public Task<int> GetUnreadMessageCountAsync(Guid userId) =>
        userChatReadStatusRepository.GetUnreadMessagesCountAsync(userId);

    public Task<int> GetUnreadMessageCountAsync(Guid userId, Guid chatId) =>
        userChatReadStatusRepository.GetUnreadMessagesCountAsync(userId, chatId);

    public Task<UserChatReadStatusBo> UpdateReadStatusAsync(Guid readStatusId) => 
        userChatReadStatusRepository.UpdateAsync(readStatusId);

    public async Task<UserChatReadStatusBo> UpdateReadStatusAsync(Guid userId, Guid chatId)
    {
        var res = await userChatReadStatusRepository.UpdateAsync(userId, chatId);

        var countNewMessagesSender = await GetUnreadMessageCountAsync(userId, chatId);

        await chatHub.Clients.Group(userId.ToString()).SendAsync("NewUnreadChatMessage", chatId, countNewMessagesSender);

        var totalCountNewMessagesSender = await GetUnreadMessageCountAsync(userId);
        await chatHub.Clients.Group(userId.ToString()).SendAsync("NewTotalUnreadChatMessage", totalCountNewMessagesSender);

        return res;
    }
    public Task DeleteReadStatusAsync(Guid uid) =>
        userChatReadStatusRepository.DeleteAsync(uid);
}
