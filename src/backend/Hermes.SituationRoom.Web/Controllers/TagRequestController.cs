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
public class TagRequestController(IControllerInfrastructure infra, ITagRequestService tagRequestService)
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

    [HttpPost("internal/tag-request")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG_REQUEST])]
    [ProducesResponseType(typeof(TagRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TagRequestDto>> CreateTagRequest([FromBody] CreateTagRequestDto createTagRequestDto)
    {
        var userId = GetUserIdFromContext();
        var tagRequest = await tagRequestService.CreateTagRequestAsync(userId, createTagRequestDto.Name);
        return CreatedAtAction(nameof(GetMyTagRequests), new { }, tagRequest);
    }

    [HttpGet("internal/tag-request/my-requests")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG_REQUEST])]
    [ProducesResponseType(typeof(IReadOnlyList<TagRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagRequestDto>>> GetMyTagRequests()
    {
        var userId = GetUserIdFromContext();
        var tagRequests = await tagRequestService.GetMyTagRequestsAsync(userId);
        return Ok(tagRequests);
    }

    [HttpGet("internal/tag-request/pending")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG_REQUEST])]
    [ProducesResponseType(typeof(IReadOnlyList<TagRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagRequestDto>>> GetPendingTagRequests()
    {
        var tagRequests = await tagRequestService.GetPendingTagRequestsAsync();
        return Ok(tagRequests);
    }

    [HttpPut("internal/tag-request/{uid:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG_REQUEST])]
    [ProducesResponseType(typeof(TagRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagRequestDto>> ApproveTagRequest(Guid uid)
    {
        var reviewerUid = GetUserIdFromContext();
        try
        {
            var tagRequest = await tagRequestService.ApproveTagRequestAsync(uid, reviewerUid);
            return Ok(tagRequest);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("internal/tag-request/{uid:guid}/reject")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG_REQUEST])]
    [ProducesResponseType(typeof(TagRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TagRequestDto>> RejectTagRequest(Guid uid)
    {
        var reviewerUid = GetUserIdFromContext();
        try
        {
            var tagRequest = await tagRequestService.RejectTagRequestAsync(uid, reviewerUid);
            return Ok(tagRequest);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
