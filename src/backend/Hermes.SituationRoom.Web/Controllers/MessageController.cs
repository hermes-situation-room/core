#nullable enable
namespace Hermes.SituationRoom.Api.Controllers;

using System.Net.Mime;
using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class MessageController(IControllerInfrastructure controllerInfrastructure, IMessageService messageService) : SituationRoomControllerBase(controllerInfrastructure)
{
    private Guid GetUserIdFromContext()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found or invalid.");
        }
        return userId;
    }

    [HttpPost("internal/message")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMessageAsync([FromBody] CreateMessageRequestDto createMessageDto)
    {
        var newMessageId = await messageService.AddAsync(createMessageDto);
        return Ok(newMessageId);
    }
    
    [HttpGet("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(MessageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageDto>> GetMessageAsync([FromRoute] Guid messageId)
    {
        var message = await messageService.GetMessageAsync(messageId, GetUserIdFromContext());
        return Ok(message);
    }
    
    [HttpGet("internal/message/get-messages-by-chat/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(IReadOnlyList<MessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessagesByChatAsync([FromRoute] Guid chatId)
    {
        var messages = await messageService.GetMessagesByChatAsync(chatId, GetUserIdFromContext());
        return Ok(messages);
    }

    [HttpPost("internal/message/{messageId:guid}/mark-viewed")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkViewedAsync([FromRoute] Guid messageId)
    {
        var marked = await messageService.MarkViewedAsync(messageId, GetUserIdFromContext());
        return marked ? NoContent() : NotFound();
    }

    [HttpPost("internal/message/purge-expired")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> PurgeExpiredAsync()
    {
        return Ok(await messageService.PurgeExpiredAsync());
    }
    
    [HttpPut("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMessageAsync(
        [FromRoute] Guid messageId,
        [FromBody] UpdateMessageRequestDto updateDto)
    {
        await messageService.UpdateAsync(messageId, updateDto.Content);
        return NoContent();
    }

    [HttpDelete("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid messageId)
    {
        await messageService.DeleteAsync(messageId);
        return NoContent();
    }
}
