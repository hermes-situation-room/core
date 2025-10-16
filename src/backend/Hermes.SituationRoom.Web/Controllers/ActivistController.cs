namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Hermes.SituationRoom.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class ActivistController(IControllerInfrastructure infra, IActivistService activistService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(ActivistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivistDto>> GetActivist(Guid uid) =>
        Ok(await activistService.GetActivistAsync(uid));

    [HttpGet("internal/activist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(IReadOnlyList<ActivistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivistDto>>> GetActivists() =>
        Ok(await activistService.GetActivistsAsync());

    [HttpPost("internal/activist/")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateActivist([FromBody] CreateActivistRequestDto createActivistDto) =>
        Ok(await activistService.CreateActivistAsync(createActivistDto));

    [HttpPut("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(ActivistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivistDto>> UpdateActivist([FromBody] UpdateActivistRequestDto updateActivistDto, Guid uid) =>
        Ok(await activistService.UpdateActivistAsync(updateActivistDto with { Uid = uid }));

    [HttpPut("internal/activist/visibilty/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(ActivistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivistDto>> UpdateActivistVisibility(
        [FromRoute] Guid uid,
        [FromBody] UpdateActivistPrivacyLevelRequestDto updateDto)
    {
        await activistService.UpdateActivistVisibilityAsync(uid, updateDto);
        return NoContent();
    }

    [HttpDelete("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteActivist(Guid uid)
    {
        await activistService.DeleteActivistAsync(uid);
        return NoContent();
    }
}
