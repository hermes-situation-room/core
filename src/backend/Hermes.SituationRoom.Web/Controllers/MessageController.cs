#nullable enable
namespace Hermes.SituationRoom.Api.Controllers;

using Shared.BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Base;
using Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using Configurations;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class MessageController(IControllerInfrastructure controllerInfrastructure, IMessageService messageService) : SituationRoomControllerBase(controllerInfrastructure)
{
    [HttpPost("internal/message")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(MessageBo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMessageAsync([FromBody] NewMessageDto newMessage)
    {
        var newMessageId = await messageService.AddAsync(newMessage);
        return Ok(new { id = newMessageId });
    }
    
    [HttpGet("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(MessageBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageBo>> GetMessageAsync([FromRoute] Guid messageId)
    {
        try
        {
            var message = await messageService.GetMessageAsync(messageId);
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("internal/message/get-messages-by-chat/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(IReadOnlyList<MessageBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MessageBo>>> GetMessagesByChatAsync([FromRoute] Guid chatId)
    {
        var messages = await messageService.GetMessagesByChatAsync(chatId);
        return Ok(messages);
    }
    
    [HttpPut("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMessageAsync(
        [FromRoute] Guid messageId,
        [FromBody] UpdateMessageDto updateDto)
    {
        try
        {
            await messageService.UpdateAsync(messageId, updateDto.Content);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("internal/message/{messageId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid messageId)
    {
        await messageService.DeleteAsync(messageId);
        return NoContent();
    }
}