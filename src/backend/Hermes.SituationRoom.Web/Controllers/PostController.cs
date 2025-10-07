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
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<PostBo>> GetPost(Guid uid) => Ok(await postService.GetPostAsync(uid));

    [HttpGet("internal/post/user/{userUid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<PostBo>> GetUserPosts(Guid userUid) =>
        Ok(await postService.GetUserPostsAsync(userUid));

    [HttpGet("internal/post/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<IReadOnlyList<PostBo>>> GetPosts() => Ok(await postService.GetPostsAsync());

    [HttpPost("internal/post/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] CreatePostDto createPostDto) =>
        Ok(await postService.CreatePostAsync(createPostDto));

    [HttpPut("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<PostBo>> UpdatePost([FromBody] PostBo postBo) =>
        Ok(await postService.UpdatePostAsync(postBo));

    [HttpDelete("internal/post/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult> DeletePost(Guid uid)
    {
        await postService.DeletePostAsync(uid);
        return NoContent();
    }
}
