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

    [HttpGet("internal/post/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetPostsByTags([FromQuery] string tags) =>
        Ok(await postService.GetPostsByTagsAsync(tags));

    [HttpGet("internal/post/activist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetActivistPostsByTags([FromQuery] string tags) =>
        Ok(await postService.GetActivistPostsByTagsAsync(tags));

    [HttpGet("internal/post/journalist/by-tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetJournalistPostsByTags([FromQuery] string tags) =>
        Ok(await postService.GetJournalistPostsByTagsAsync(tags));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetUserPosts(Guid userUid) =>
        Ok(await postService.GetUserPostsAsync(userUid));

    [HttpGet("internal/post/activist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetAllActivistPosts() => Ok(await postService.GetAllActivistPostsAsync());

    [HttpGet("internal/post/journalist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetAllAJournalistPosts() =>
        Ok(await postService.GetAllJournalistPostsAsync());

    [HttpGet("internal/post/")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostBo>>> GetPosts() => Ok(await postService.GetPostsAsync());

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
    public async Task<ActionResult<PostWithTagsBo>> UpdatePost([FromBody] PostWithTagsBo postBo, Guid uid) =>
        Ok(await postService.UpdatePostAsync(postBo with { Uid = uid, }));

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
