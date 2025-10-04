namespace Hermes.SituationRoom.Api.Controllers;

using Shared.BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Base;
using Domain.Interface;
using System.ComponentModel.DataAnnotations;
using Configurations;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public class ChatController(IControllerInfrastructure controllerInfrastructure, IChatService chatService) : SituationRoomControllerBase(controllerInfrastructure)
{
    [HttpPost("internal/chats")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(ChatBo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddAsync([FromBody] ChatBo newChatBo)
    {
        var newChatId = await chatService.AddAsync(newChatBo);
        
        return Ok(new { id = newChatId });
    }
    
    [HttpGet("internal/chats/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(ChatBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatBo>> GetChatAsync([FromRoute] Guid chatId)
    {
        try
        {
            var chat = await chatService.GetChatAsync(chatId);
            return Ok(chat);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("internal/chats/by-user-pair")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(ChatBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatBo>> GetChatByUserPairAsync([FromQuery, Required] Guid user1Id, [FromQuery, Required] Guid user2Id)
    {
        try
        {
            var chat = await chatService.GetChatByUserPairAsync(user1Id, user2Id);
            return Ok(chat);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    [HttpGet("internal/chats/by-user/{userId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(typeof(IReadOnlyList<ChatBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChatBo>>> GetChatsByUserAsync([FromRoute] Guid userId)
    {
        var chats = await chatService.GetChatsByUserAsync(userId);
        return Ok(chats);
    }
    
    [HttpDelete("internal/chats/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid chatId)
    {
        await chatService.DeleteAsync(chatId);
        return NoContent();
    }
}