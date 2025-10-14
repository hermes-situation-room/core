namespace Hermes.SituationRoom.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using Base;
using Domain.Interfaces;
using Shared.BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Configurations;
using Swashbuckle.AspNetCore.Annotations;

public class UserChatReadStatusController(IControllerInfrastructure infra, IUserChatReadStatusService userChatStatusService) :
        SituationRoomControllerBase(infra)
{
    [HttpGet("internal/userChatReadStatus/total")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_READ_STATUS])]
    [ProducesResponseType(typeof(UserChatStatusBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<UserChatStatusBo> GetTotalUserChatStatusAsync([Required][FromQuery] Guid userId)
    {
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId);
        return new (userId, res);
    }

    [HttpGet("internal/userChatReadStatus/byChat")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_READ_STATUS])]
    [ProducesResponseType(typeof(UserChatStatusBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<UserChatStatusBo> GetUserChatStatusByChatAsync([Required][FromQuery] Guid userId, [Required][FromQuery] Guid chatId)
    {
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId, chatId);
        return new (chatId, res);
    }
}
