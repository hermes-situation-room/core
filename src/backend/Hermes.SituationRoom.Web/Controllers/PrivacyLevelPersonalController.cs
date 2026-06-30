namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

[Authorize]
public class PrivacyLevelPersonalController(IControllerInfrastructure infra, IPrivacyLevelPersonalService privacyLevelPersonalService)
    : SituationRoomControllerBase(infra)
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

    [HttpGet("internal/privacylevelpersonal")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(PrivacyLevelPersonalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalDto>> GetPrivacyLevelPersonal([FromQuery] Guid ownerUid, [FromQuery] Guid consumerUid)
    {
        if (ownerUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        return Ok(await privacyLevelPersonalService.GetPrivacyLevelPersonalAsync(ownerUid, consumerUid));
    }

    [HttpPost("internal/privacylevelpersonal/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> CreatePrivacyLevelPersonal([FromBody] CreatePrivacyLevelPersonalRequestDto createPrivacyLevelPersonalDto)
    {
        if (createPrivacyLevelPersonalDto.OwnerUid != GetUserIdFromContext())
        {
            return Forbid();
        }
        return Ok(await privacyLevelPersonalService.CreatePrivacyLevelPersonalAsync(createPrivacyLevelPersonalDto));
    }

    [HttpPut("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(PrivacyLevelPersonalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalDto>> UpdatePrivacyLevelPersonal([FromBody] UpdatePrivacyLevelPersonalRequestDto updatePrivacyLevelPersonalDto, Guid uid)
    {
        try
        {
            return Ok(await privacyLevelPersonalService.UpdatePrivacyLevelPersonalAsync(GetUserIdFromContext(), updatePrivacyLevelPersonalDto with { Uid = uid }));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeletePrivacyLevelPersonal(Guid uid)
    {
        try
        {
            await privacyLevelPersonalService.DeletePrivacyLevelPersonalAsync(GetUserIdFromContext(), uid);
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
