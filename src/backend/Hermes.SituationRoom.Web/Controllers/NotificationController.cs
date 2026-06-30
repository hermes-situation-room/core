using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Api.Controllers.Base;
using Hermes.SituationRoom.Api.Configurations;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace Hermes.SituationRoom.Api.Controllers;

[ApiController]
[Authorize]
[Route("services/api/internal/notification")]
public class NotificationController(IControllerInfrastructure infra, INotificationService notificationService)
    : SituationRoomControllerBase(infra)
{
    private readonly INotificationService _notificationService = notificationService;

    private Guid GetUserIdFromContext()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }
        return userId;
    }

    [HttpGet("user/{userUid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(IEnumerable<NotificationBo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NotificationBo>>> GetByUserUid(Guid userUid)
    {
        if (userUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        var notifications = await _notificationService.GetByUserUidAsync(userUid);
        return Ok(notifications);
    }

    [HttpGet("user/{userUid}/unread")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(IEnumerable<NotificationBo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<NotificationBo>>> GetUnreadByUserUid(Guid userUid)
    {
        if (userUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        var notifications = await _notificationService.GetUnreadByUserUidAsync(userUid);
        return Ok(notifications);
    }

    [HttpGet("user/{userUid}/unread/count")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userUid)
    {
        if (userUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        var count = await _notificationService.GetUnreadCountAsync(userUid);
        return Ok(count);
    }

    [HttpGet("{uid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(NotificationBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationBo>> GetById(Guid uid)
    {
        var notification = await _notificationService.GetByIdAsync(uid);
        if (notification == null || notification.UserUid != GetUserIdFromContext())
        {
            return NotFound();
        }
        return Ok(notification);
    }

    [HttpPut("{uid}/read")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid uid)
    {
        var notification = await _notificationService.GetByIdAsync(uid);
        if (notification == null || notification.UserUid != GetUserIdFromContext())
        {
            return NotFound();
        }
        await _notificationService.MarkAsReadAsync(uid);
        return Ok();
    }

    [HttpPut("user/{userUid}/read-all")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkAllAsRead(Guid userUid)
    {
        if (userUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        await _notificationService.MarkAllAsReadAsync(userUid);
        return Ok();
    }

    [HttpDelete("{uid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid uid)
    {
        var notification = await _notificationService.GetByIdAsync(uid);
        if (notification == null || notification.UserUid != GetUserIdFromContext())
        {
            return NotFound();
        }
        await _notificationService.DeleteAsync(uid);
        return NoContent();
    }
}
