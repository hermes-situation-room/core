namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PostController(IControllerInfrastructure infra, IPostService postService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostBo>> GetPost(Guid uid) => Ok(await postService.GetPostAsync(uid));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostBo>> GetUserPosts(Guid userUid) =>
        Ok(await postService.GetUserPostsAsync(userUid));

    [HttpGet("internal/post/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostBo>>> GetPosts() => Ok(await postService.GetPostsAsync());

    [HttpPost("internal/post/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostDto createPostDto) =>
        Ok(await postService.CreatePostAsync(createPostDto));

    [HttpPut("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostBo>> UpdatePost([FromBody] PostBo postBo, Guid uid) =>
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
