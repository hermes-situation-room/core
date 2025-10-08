namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class ActivistController(IControllerInfrastructure infra, IActivistService activistService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(ActivistBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivistBo>> GetActivist(Guid uid) =>
        Ok(await activistService.GetActivistAsync(uid));

    [HttpGet("internal/activist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(IReadOnlyList<ActivistBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ActivistBo>>> GetActivists() =>
        Ok(await activistService.GetActivistsAsync());

    [HttpPost("internal/activist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateActivist([FromBody] ActivistBo activistBo) =>
        Ok(await activistService.CreateActivistAsync(activistBo));

    [HttpPut("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_ACTIVIST])]
    [ProducesResponseType(typeof(ActivistBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivistBo>> UpdateActivist([FromBody] ActivistBo activistBo, Guid uid) =>
        Ok(await activistService.UpdateActivistAsync(activistBo with { Uid = uid, }));

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
