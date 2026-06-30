namespace Hermes.SituationRoom.Api.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Base;
using Domain.Interfaces;
using Shared.BusinessObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Configurations;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class UserChatReadStatusController(IControllerInfrastructure infra, IUserChatReadStatusService userChatStatusService) :
        SituationRoomControllerBase(infra)
{
    private Guid GetUserIdFromContext()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }
        return userId;
    }

    [HttpGet("internal/userChatReadStatus/total")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_READ_STATUS])]
    [ProducesResponseType(typeof(UserChatStatusBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserChatStatusBo>> GetTotalUserChatStatusAsync([Required][FromQuery] Guid userId)
    {
        if (userId != GetUserIdFromContext())
        {
            return Forbid();
        }
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId);
        return new UserChatStatusBo(userId, res);
    }

    [HttpGet("internal/userChatReadStatus/byChat")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_READ_STATUS])]
    [ProducesResponseType(typeof(UserChatStatusBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserChatStatusBo>> GetUserChatStatusByChatAsync([Required][FromQuery] Guid userId, [Required][FromQuery] Guid chatId)
    {
        if (userId != GetUserIdFromContext())
        {
            return Forbid();
        }
        var res = await userChatStatusService.GetUnreadMessageCountAsync(userId, chatId);
        return new UserChatStatusBo(chatId, res);
    }
}
