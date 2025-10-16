namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class CommentController(IControllerInfrastructure infra, ICommentService commentService)
    : SituationRoomControllerBase(infra)
{
    [AllowAnonymous]
    [HttpGet("internal/post/{postUid:guid}/comment")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(IReadOnlyList<CommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CommentDto>>> GetPostComments(Guid postUid) =>
        Ok(await commentService.GetPostCommentsAsync(postUid));

    [HttpPost("internal/comment/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> CreateComment([FromBody] CreateCommentRequestDto createCommentDto) =>
        Ok(await commentService.CreateCommentAsync(createCommentDto));

    [HttpPut("internal/comment/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_COMMENT])]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> UpdateComment([FromBody] UpdateCommentRequestDto updateCommentDto, Guid uid) =>
        Ok(await commentService.UpdateCommentAsync(updateCommentDto with { Uid = uid }));

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
