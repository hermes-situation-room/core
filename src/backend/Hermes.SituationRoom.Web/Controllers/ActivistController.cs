namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class ActivistController(IControllerInfrastructure infra, IActivistService activistService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<ActivistBo>> GetActivist(Guid uid) =>
        Ok(await activistService.GetActivistAsync(uid));

    [HttpGet("internal/activist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<IReadOnlyList<ActivistBo>>> GetActivists() =>
        Ok(await activistService.GetActivistsAsync());

    [HttpPost("internal/activist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<Guid>> CreateActivist([FromBody] ActivistBo activistBo) =>
        Ok(await activistService.CreateActivistAsync(activistBo));

    [HttpPut("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<ActivistBo>> UpdateActivist([FromBody] ActivistBo activistBo) =>
        Ok(await activistService.UpdateActivistAsync(activistBo));

    [HttpDelete("internal/activist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult> DeleteActivist(Guid uid)
    {
        await activistService.DeleteActivistAsync(uid);
        return NoContent();
    }
}
