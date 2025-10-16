namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class JournalistController(IControllerInfrastructure infra, IJournalistService journalistService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_JOURNALIST])]
    [ProducesResponseType(typeof(JournalistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalistDto>> GetJournalist(Guid uid) =>
        Ok(await journalistService.GetJournalistAsync(uid));

    [HttpGet("internal/journalist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_JOURNALIST])]
    [ProducesResponseType(typeof(IReadOnlyList<JournalistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JournalistDto>>> GetJournalists() =>
        Ok(await journalistService.GetJournalistsAsync());

    [HttpPost("internal/journalist/")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_JOURNALIST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateJournalist([FromBody] CreateJournalistRequestDto createJournalistDto) =>
        Ok(await journalistService.CreateJournalistAsync(createJournalistDto));

    [HttpPut("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_JOURNALIST])]
    [ProducesResponseType(typeof(JournalistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalistDto>> UpdateJournalist([FromBody] UpdateJournalistRequestDto updateJournalistDto, Guid uid) =>
        Ok(await journalistService.UpdateJournalistAsync(updateJournalistDto with { Uid = uid }));

    [HttpDelete("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_JOURNALIST])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteJournalist(Guid uid)
    {
        await journalistService.DeleteJournalistAsync(uid);
        return NoContent();
    }
}
