namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PostController(IControllerInfrastructure infra, IPostService postService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{uid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostWithTagsBo>> GetPost(Guid uid) => Ok(await postService.GetPostAsync(uid));

    [HttpGet("internal/post/activist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsBo>> GetActivistPostsByTags([FromQuery] string tags, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query) =>
        Ok(await postService.GetActivistPostsByTagsAsync(tags, limit ?? 12, offset ?? 0, query));

    [HttpGet("internal/post/journalist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsBo>> GetJournalistPostsByTags([FromQuery] string tags, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query) =>
        Ok(await postService.GetJournalistPostsByTagsAsync(tags, limit ?? 12, offset ?? 0, query));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsBo>> GetUserPosts(Guid userUid, [FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query) =>
        Ok(await postService.GetUserPostsAsync(userUid, limit ?? 12, offset ?? 0, query));

    [HttpGet("internal/post/activist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsBo>> GetAllActivistPosts([FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query) => 
        Ok(await postService.GetAllActivistPostsAsync(limit ?? 12, offset ?? 0, query));

    [HttpGet("internal/post/journalist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostWithTagsBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostWithTagsBo>> GetAllJournalistPosts([FromQuery] int? limit, [FromQuery] int? offset, [FromQuery] string? query) =>
        Ok(await postService.GetAllJournalistPostsAsync(limit ?? 12, offset ?? 0, query));

    [HttpPost("internal/post/")]
    [Authorize]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostDto createPostDto) =>
        Ok(await postService.CreatePostAsync(createPostDto));

    [HttpPut("internal/post/{uid:guid}")]
    [Authorize]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PostWithTagsBo>> UpdatePost([FromBody] PostWithTagsBo postBo, Guid uid)
    {
        var existingPost = await postService.GetPostAsync(uid);
        if (existingPost == null)
            return NotFound();

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        if (existingPost.CreatorUid != userId)
            return Forbid();

        return Ok(await postService.UpdatePostAsync(postBo with { Uid = uid, }));
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
}
