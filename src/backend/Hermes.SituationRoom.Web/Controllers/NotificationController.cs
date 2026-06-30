using Microsoft.AspNetCore.Mvc;
using Hermes.SituationRoom.Domain.Interfaces;
using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Hermes.SituationRoom.Api.Controllers.Base;
using Hermes.SituationRoom.Api.Configurations;
using Swashbuckle.AspNetCore.Annotations;

namespace Hermes.SituationRoom.Api.Controllers;

[ApiController]
[Route("services/api/internal/notification")]
public class NotificationController(IControllerInfrastructure infra, INotificationService notificationService)
    : SituationRoomControllerBase(infra)
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet("user/{userUid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(IEnumerable<NotificationBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationBo>>> GetByUserUid(Guid userUid)
    {
        var notifications = await _notificationService.GetByUserUidAsync(userUid);
        return Ok(notifications);
    }

    [HttpGet("user/{userUid}/unread")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(IEnumerable<NotificationBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificationBo>>> GetUnreadByUserUid(Guid userUid)
    {
        var notifications = await _notificationService.GetUnreadByUserUidAsync(userUid);
        return Ok(notifications);
    }

    [HttpGet("user/{userUid}/unread/count")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetUnreadCount(Guid userUid)
    {
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
        if (notification == null)
        {
            return NotFound();
        }
        return Ok(notification);
    }

    [HttpPost]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(typeof(NotificationBo), StatusCodes.Status201Created)]
    public async Task<ActionResult<NotificationBo>> Create([FromBody] CreateNotificationDto createNotificationDto)
    {
        var notification = new NotificationBo(
            Guid.NewGuid(),
            createNotificationDto.UserUid,
            createNotificationDto.Title,
            createNotificationDto.Message,
            createNotificationDto.Type,
            false,
            DateTime.UtcNow,
            createNotificationDto.RelatedEntityUid,
            createNotificationDto.RelatedEntityType
        );

        var createdNotification = await _notificationService.CreateAsync(notification);
        return CreatedAtAction(nameof(GetById), new { uid = createdNotification.Uid }, createdNotification);
    }

    [HttpPut("{uid}/read")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(Guid uid)
    {
        await _notificationService.MarkAsReadAsync(uid);
        return Ok();
    }

    [HttpPut("user/{userUid}/read-all")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(Guid userUid)
    {
        await _notificationService.MarkAllAsReadAsync(userUid);
        return Ok();
    }

    [HttpDelete("{uid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_NOTIFICATION])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid uid)
    {
        await _notificationService.DeleteAsync(uid);
        return NoContent();
    }
}
