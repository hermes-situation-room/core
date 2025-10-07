namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PrivacyLevelPersonalController(IControllerInfrastructure infra, IPrivacyLevelPersonalService privacyLevelPersonalService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/privacylevelpersonal/{ownerUid:guid}/{consumerUid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(ChatBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalBo>> GetPrivacyLevelPersonal(Guid ownerUid, Guid consumerUid) => 
        Ok(await privacyLevelPersonalService.GetPrivacyLevelPersonalAsync(ownerUid, consumerUid));

    [HttpPost("internal/privacylevelpersonal/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(PrivacyLevelPersonalBo), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePrivacyLevelPersonal([FromBody] PrivacyLevelPersonalBo privacyLevelPersonalBo) =>
        Ok(await privacyLevelPersonalService.CreatePrivacyLevelPersonalAsync(privacyLevelPersonalBo));

    [HttpPut("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(ChatBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalBo>> UpdatePrivacyLevelPersonal([FromBody] PrivacyLevelPersonalBo privacyLevelPersonalBo, Guid uid) =>
        Ok(await privacyLevelPersonalService.UpdatePrivacyLevelPersonalAsync(privacyLevelPersonalBo with { Uid = uid, }));

    [HttpDelete("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeletePrivacyLevelPersonal(Guid uid)
    {
        await privacyLevelPersonalService.DeletePrivacyLevelPersonalAsync(uid);
        return NoContent();
    }
}
