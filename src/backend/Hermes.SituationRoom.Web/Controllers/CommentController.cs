namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class CommentController(IControllerInfrastructure infra, ICommentService commentService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/post/{postUid:guid}/comment")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(IReadOnlyList<CommentBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<CommentBo>> GetPostComments(Guid postUid) =>
        Ok(await commentService.GetPostCommentsAsync(postUid));

    [HttpPost("internal/comment/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(IReadOnlyList<CommentBo>), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateComment([FromBody] CreateCommentDto createCommentDto) =>
        Ok(await commentService.CreateCommentAsync(createCommentDto));

    [HttpPut("internal/comment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(CommentBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentBo>> UpdateComment([FromBody] CommentBo commentBo, Guid uid) =>
        Ok(await commentService.UpdateCommentAsync(commentBo with { Uid = uid, }));

    [HttpDelete("internal/comment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteComment(Guid uid)
    {
        await commentService.DeleteCommentAsync(uid);
        return NoContent();
    }
}
