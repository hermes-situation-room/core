#nullable enable
namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PostController(IControllerInfrastructure infra, IPostService postService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{uid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostWithTagsDto>> GetPost(Guid uid) => Ok(await postService.GetPostAsync(uid));

    [HttpGet("internal/post/activist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsDto>> GetActivistPostsByTags([FromQuery] string tags, [FromQuery] string privacyLevel, [FromQuery] int? limit, [FromQuery] int? offset)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var userRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

        Guid.TryParse(userIdClaim?.Value, out var userId);

        return Ok(await postService.GetActivistPostsByTagsAsync(tags, userId, userRoleClaim?.Value, limit ?? 12, offset ?? 0));
    }

    [HttpGet("internal/post/journalist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostWithTagsDto>>> GetJournalistPostsByTags([FromQuery] string tags, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query, [FromQuery] string? sortBy) =>
        Ok(await postService.GetJournalistPostsByTagsAsync(tags, limit ?? 12, offset ?? 0, query, sortBy));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostWithTagsDto>>> GetUserPosts(Guid userUid, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query, [FromQuery] string? sortBy) =>
        Ok(await postService.GetUserPostsAsync(userUid, limit ?? 12, offset ?? 0, query, sortBy));

    [HttpGet("internal/post/activist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsDto>> GetAllActivistPosts([FromQuery] int? limit, [FromQuery] int? offset)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        var userRoleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

        Guid.TryParse(userIdClaim?.Value, out var userId);

        return Ok(await postService.GetAllActivistPostsAsync(userId, userRoleClaim?.Value, limit ?? 12, offset ?? 0));
    }

    [HttpGet("internal/post/journalist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostWithTagsDto>>> GetAllJournalistPosts([FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query, [FromQuery] string? sortBy) =>
        Ok(await postService.GetAllJournalistPostsAsync(limit ?? 12, offset ?? 0, query, sortBy));

    [HttpPost("internal/post/")]
    [Authorize]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostRequestDto createPostDto) =>
        Ok(await postService.CreatePostAsync(createPostDto));

    [HttpPut("internal/post/{uid:guid}")]
    [Authorize]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PostWithTagsDto>> UpdatePost([FromBody] UpdatePostRequestDto updatePostDto, Guid uid)
    {
        var existingPost = await postService.GetPostAsync(uid);
        if (existingPost == null)
            return NotFound();

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        if (existingPost.CreatorUid != userId)
            return Forbid();

        return Ok(await postService.UpdatePostAsync(updatePostDto with { Uid = uid }));
    }

    [HttpDelete("internal/post/{uid:guid}")]
    [Authorize]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePost(Guid uid)
    {
        await postService.DeletePostAsync(uid);
        return NoContent();
    }

    [HttpGet("internal/post/privacies")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsDto>> GetPostPrivacies() => Ok(await postService.GetPostPrivaciesAsync());
}
