namespace Hermes.SituationRoom.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Hermes.SituationRoom.Api.Controllers.Base;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

public class UserChatReadStatusController(IControllerInfrastructure infra, IUserChatReadStatusService userChatStatusService) :
        SituationRoomControllerBase(infra)
{
    [HttpGet("internal/userChatReadStatus/total")]
    public async Task<UserChatStatusBo> GetTotalUserChatStatusAsync([Required][FromQuery] Guid userId)
    {
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId);
        return new UserChatStatusBo(userId, res);
    }

    [HttpGet("internal/userChatReadStatus/byChat")]
    public async Task<UserChatStatusBo> GetUserChatStatusByChatAsync([Required][FromQuery] Guid userId, [Required][FromQuery] Guid chatId)
    {
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId, chatId);
        return new UserChatStatusBo(chatId, res);
    }
}
