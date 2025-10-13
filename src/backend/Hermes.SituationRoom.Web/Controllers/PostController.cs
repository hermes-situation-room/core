namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PostController(IControllerInfrastructure infra, IPostService postService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostWithTagsBo>> GetPost(Guid uid) => Ok(await postService.GetPostAsync(uid));

    [HttpGet("internal/post/activist/by-tags")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetActivistPostsByTags([FromQuery] string tags, [FromQuery] int? limit, [FromQuery] int? offset) =>
        Ok(await postService.GetActivistPostsByTagsAsync(tags, limit ?? 12, offset ?? 0));

    [HttpGet("internal/post/journalist/by-tags")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetJournalistPostsByTags([FromQuery] string tags, [FromQuery] int? limit, [FromQuery] int? offset) =>
        Ok(await postService.GetJournalistPostsByTagsAsync(tags, limit ?? 12, offset ?? 0));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetUserPosts(Guid userUid, [FromQuery] int? limit, [FromQuery] int? offset) =>
        Ok(await postService.GetUserPostsAsync(userUid, limit ?? 12, offset ?? 0));

    [HttpGet("internal/post/activist")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetAllActivistPosts([FromQuery] int? limit, [FromQuery] int? offset) => 
        Ok(await postService.GetAllActivistPostsAsync(limit ?? 12, offset ?? 0));

    [HttpGet("internal/post/journalist")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetAllAJournalistPosts([FromQuery] int? limit, [FromQuery] int? offset) =>
        Ok(await postService.GetAllJournalistPostsAsync(limit ?? 12, offset ?? 0));

    [HttpPost("internal/post/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostDto createPostDto) =>
        Ok(await postService.CreatePostAsync(createPostDto));

    [HttpPut("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostWithTagsBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostWithTagsBo>> UpdatePost([FromBody] PostWithTagsBo postBo, Guid uid) =>
        Ok(await postService.UpdatePostAsync(postBo with { Uid = uid, }));

    [HttpDelete("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeletePost(Guid uid)
    {
        await postService.DeletePostAsync(uid);
        return NoContent();
    }
}
