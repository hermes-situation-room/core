namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class CommentController(IControllerInfrastructure infra, ICommentService commentService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/comment/post/{postUid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<CommentBo>> GetPostComments(Guid postUid) =>
        Ok(await commentService.GetPostCommentsAsync(postUid));

    [HttpPost("internal/comment/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<Guid>> CreateComment([FromBody] CreateCommentDto createCommentDto) =>
        Ok(await commentService.CreateCommentAsync(createCommentDto));

    [HttpPut("internal/comment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<CommentBo>> UpdateComment([FromBody] CommentBo commentBo) =>
        Ok(await commentService.UpdateCommentAsync(commentBo));

    [HttpDelete("internal/comment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult> DeleteComment(Guid uid)
    {
        await commentService.DeleteCommentAsync(uid);
        return NoContent();
    }
}
