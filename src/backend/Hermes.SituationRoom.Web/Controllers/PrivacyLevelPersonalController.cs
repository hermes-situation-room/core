namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

public class PrivacyLevelPersonalController(IControllerInfrastructure infra, IPrivacyLevelPersonalService privacyLevelPersonalService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/privacylevelpersonal")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(PrivacyLevelPersonalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalDto>> GetPrivacyLevelPersonal([FromQuery] Guid ownerUid, [FromQuery] Guid consumerUid) =>
        Ok(await privacyLevelPersonalService.GetPrivacyLevelPersonalAsync(ownerUid, consumerUid));

    [HttpPost("internal/privacylevelpersonal/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePrivacyLevelPersonal([FromBody] CreatePrivacyLevelPersonalRequestDto createPrivacyLevelPersonalDto) =>
        Ok(await privacyLevelPersonalService.CreatePrivacyLevelPersonalAsync(createPrivacyLevelPersonalDto));

    [HttpPut("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(typeof(PrivacyLevelPersonalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrivacyLevelPersonalDto>> UpdatePrivacyLevelPersonal([FromBody] UpdatePrivacyLevelPersonalRequestDto updatePrivacyLevelPersonalDto, Guid uid) =>
        Ok(await privacyLevelPersonalService.UpdatePrivacyLevelPersonalAsync(updatePrivacyLevelPersonalDto with { Uid = uid }));

    [HttpDelete("internal/privacylevelpersonal/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_PRIVACY_LEVEL])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeletePrivacyLevelPersonal(Guid uid)
    {
        await privacyLevelPersonalService.DeletePrivacyLevelPersonalAsync(uid);
        return NoContent();
    }
}
