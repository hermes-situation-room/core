#nullable enable
namespace Hermes.SituationRoom.Api.Controllers;

using System.Net.Mime;
using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class MessageController(IControllerInfrastructure controllerInfrastructure, IMessageService messageService) : SituationRoomControllerBase(controllerInfrastructure)
{
    [HttpPost("internal/message")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMessageAsync([FromBody] NewMessageDto newMessage)
    {
        var newMessageId = await messageService.AddAsync(newMessage);
        return Ok(newMessageId);
    }
    
    [HttpGet("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(MessageBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageBo>> GetMessageAsync([FromRoute] Guid messageId)
    {
            var message = await messageService.GetMessageAsync(messageId);
            return Ok(message);
    }
    
    [HttpGet("internal/message/get-messages-by-chat/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(typeof(IReadOnlyList<MessageBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MessageBo>>> GetMessagesByChatAsync([FromRoute] Guid chatId)
    {
        var messages = await messageService.GetMessagesByChatAsync(chatId);
        return Ok(messages);
    }
    
    [HttpPut("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_MESSAGE])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMessageAsync(
        [FromRoute] Guid messageId,
        [FromBody] UpdateMessageDto updateDto)
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