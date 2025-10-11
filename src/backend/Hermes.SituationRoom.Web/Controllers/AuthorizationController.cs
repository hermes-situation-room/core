namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class AuthorizationController(IControllerInfrastructure infra, IAuthorizationService authorizationService)
    : SituationRoomControllerBase(infra)
{
    [HttpPost("internal/authorization/login/activist")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> LoginActivist(LoginActivistBo loginActivistBo)
    {
        var activistGuid = await authorizationService.LoginActivist(loginActivistBo);
        return Ok(activistGuid);
    }

    [HttpPost("internal/authorization/login/journalist")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> LoginJournalist(LoginJournalistBo loginJournalistBo)
    {
        var journalistGuid = await authorizationService.LoginJournalist(loginJournalistBo);
        return Ok(journalistGuid);
    }
}
