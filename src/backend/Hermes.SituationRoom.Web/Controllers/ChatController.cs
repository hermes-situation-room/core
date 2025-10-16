namespace Hermes.SituationRoom.Api.Controllers;

using Shared.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Base;
using System.ComponentModel.DataAnnotations;
using Configurations;
using Domain.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class ChatController(IControllerInfrastructure controllerInfrastructure, IChatService chatService)
    : SituationRoomControllerBase(controllerInfrastructure)
{
    [HttpPost("internal/chats")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CHAT])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> AddAsync([FromBody] CreateChatRequestDto createChatDto) => Ok(await chatService.AddAsync(createChatDto));

    [HttpGet("internal/chats/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CHAT])]
    [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatDto>> GetChatAsync([FromRoute] Guid chatId)
    {
        var chat = await chatService.GetChatAsync(chatId);
        return Ok(chat);
    }

    [HttpGet("internal/chats/by-user-pair")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CHAT])]
    [ProducesResponseType(typeof(ChatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatDto>> GetChatByUserPairAsync([FromQuery] [Required] Guid user1Id,
        [FromQuery] [Required] Guid user2Id
    )
    {
        var chat = await chatService.GetChatByUserPairAsync(user1Id, user2Id);
        return Ok(chat);
    }

    [HttpGet("internal/chats/by-user/{userId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CHAT])]
    [ProducesResponseType(typeof(IReadOnlyList<ChatDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ChatDto>>> GetChatsByUserAsync([FromRoute] Guid userId) =>
        Ok(await chatService.GetChatsByUserAsync(userId));

    [HttpDelete("internal/chats/{chatId:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_CHAT])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid chatId)
    {
        await chatService.DeleteAsync(chatId);
        return NoContent();
    }
}
