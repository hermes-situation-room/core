namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class PostLikeController(IControllerInfrastructure infra, IPostLikeService postLikeService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{postId:guid}/likes/stats")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(PostLikeStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PostLikeStatsDto>> GetPostLikeStats(
        [FromRoute] Guid postId,
        [FromQuery] Guid? userId) =>
        Ok(await postLikeService.GetPostLikeStatsAsync(postId, userId));

    [HttpPost("internal/post/{postId:guid}/likes/toggle")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<ActionResult<Guid>> ToggleLike(
        [FromRoute] Guid postId,
        [FromQuery] Guid userId) =>
        Ok(await postLikeService.ToggleLikeAsync(postId, userId));

    [HttpGet("internal/post/{postId:guid}/likes")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_POST])]
    [ProducesResponseType(typeof(IReadOnlyList<PostLikeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostLikeDto>>> GetPostLikes([FromRoute] Guid postId) =>
        Ok(await postLikeService.GetPostLikesAsync(postId));
}

