namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[Authorize]
public class AttachmentController(IControllerInfrastructure infra, IAttachmentService attachmentService)
    : SituationRoomControllerBase(infra)
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

    [HttpPost("internal/attachment")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ATTACHMENT])]
    [ProducesResponseType(typeof(AttachmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AttachmentDto>> Upload(
        [FromForm] IFormFile file,
        [FromForm] Guid? postId,
        [FromForm] Guid? messageId,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided.");
        }

        var ownerUid = GetUserIdFromContext();

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);

        var dto = await attachmentService.UploadAsync(
            ownerUid,
            file.FileName,
            file.ContentType,
            memoryStream.ToArray(),
            postId,
            messageId,
            cancellationToken);

        return CreatedAtAction(nameof(Download), new { uid = dto.Uid }, dto);
    }

    [HttpGet("internal/attachment/by-post/{postId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ATTACHMENT])]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> GetByPost([FromRoute] Guid postId)
    {
        var requesterUid = GetUserIdFromContext();
        var requesterRole = User.FindFirst(ClaimTypes.Role)?.Value;
        return Ok(await attachmentService.GetByPostAsync(postId, requesterUid, requesterRole));
    }

    [HttpGet("internal/attachment/by-message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ATTACHMENT])]
    [ProducesResponseType(typeof(IReadOnlyList<AttachmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> GetByMessage([FromRoute] Guid messageId)
    {
        var requesterUid = GetUserIdFromContext();
        return Ok(await attachmentService.GetByMessageAsync(messageId, requesterUid));
    }

    [HttpGet("internal/attachment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ATTACHMENT])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download([FromRoute] Guid uid, CancellationToken cancellationToken)
    {
        var requesterUid = GetUserIdFromContext();
        var requesterRole = User.FindFirst(ClaimTypes.Role)?.Value;

        try
        {
            var content = await attachmentService.DownloadAsync(uid, requesterUid, requesterRole, cancellationToken);
            if (content == null)
            {
                return NotFound();
            }

            return File(content.Bytes, content.ContentType, content.FileName);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("internal/attachment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ATTACHMENT])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] Guid uid)
    {
        var requesterUid = GetUserIdFromContext();

        try
        {
            await attachmentService.DeleteAsync(uid, requesterUid);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
