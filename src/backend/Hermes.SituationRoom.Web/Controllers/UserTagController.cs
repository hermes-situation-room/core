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
public class UserTagController(IControllerInfrastructure infra, IUserTagService userTagService)
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

    [HttpGet("internal/user-tag")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(typeof(IReadOnlyList<UserTagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserTagDto>>> GetMyTags() =>
        Ok(await userTagService.GetMyTagsAsync(GetUserIdFromContext()));

    [HttpGet("internal/user-tag/user/{taggedUserId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(typeof(IReadOnlyList<UserTagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserTagDto>>> GetMyTagsForUser([FromRoute] Guid taggedUserId) =>
        Ok(await userTagService.GetMyTagsForUserAsync(GetUserIdFromContext(), taggedUserId));

    [HttpGet("internal/user-tag/search")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(typeof(IReadOnlyList<UserTagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserTagDto>>> SearchMyTags([FromQuery] string tagName) =>
        Ok(await userTagService.SearchMyTagsAsync(GetUserIdFromContext(), tagName));

    [HttpPost("internal/user-tag")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(typeof(UserTagDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserTagDto>> CreateTag([FromBody] CreateUserTagDto createUserTagDto)
    {
        var created = await userTagService.CreateAsync(GetUserIdFromContext(), createUserTagDto);
        return CreatedAtAction(nameof(GetMyTags), created);
    }

    [HttpPut("internal/user-tag/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(typeof(UserTagDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserTagDto>> UpdateTag(
        [FromRoute] Guid uid,
        [FromBody] UpdateUserTagDto updateUserTagDto) =>
        Ok(await userTagService.UpdateAsync(GetUserIdFromContext(), uid, updateUserTagDto));

    [HttpDelete("internal/user-tag/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER_TAG])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTag([FromRoute] Guid uid)
    {
        await userTagService.DeleteAsync(GetUserIdFromContext(), uid);
        return NoContent();
    }
}
