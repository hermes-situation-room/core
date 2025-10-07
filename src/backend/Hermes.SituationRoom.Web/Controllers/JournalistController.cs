namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class JournalistController(IControllerInfrastructure infra, IJournalistService journalistService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<JournalistBo>> GetJournalist(Guid uid) =>
        Ok(await journalistService.GetJournalistAsync(uid));

    [HttpGet("internal/journalist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<IReadOnlyList<JournalistBo>>> GetJournalists() =>
        Ok(await journalistService.GetJournalistsAsync());

    [HttpPost("internal/journalist/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<Guid>> CreateJournalist([FromBody] JournalistBo journalistBo) =>
        Ok(await journalistService.CreateJournalistAsync(journalistBo));

    [HttpPut("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<JournalistBo>> UpdateJournalist([FromBody] JournalistBo journalistBo) =>
        Ok(await journalistService.UpdateJournalistAsync(journalistBo));

    [HttpDelete("internal/journalist/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult> DeleteJournalist(Guid uid)
    {
        await journalistService.DeleteJournalistAsync(uid);
        return NoContent();
    }
}
