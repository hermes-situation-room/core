namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;
using IAuthorizationService = Domain.Interfaces.IAuthorizationService;

public class AuthorizationController(IControllerInfrastructure infra, IAuthorizationService authorizationService)
    : SituationRoomControllerBase(infra)
{
    [HttpPost("internal/authorization/login/activist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> LoginActivist(LoginActivistBo loginActivistBo)
    {
        var activistGuid = await authorizationService.LoginActivist(loginActivistBo);
        return Ok(activistGuid);
    }

    [HttpPost("internal/authorization/login/journalist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Guid>> LoginJournalist(LoginJournalistBo loginJournalistBo)
    {
        var journalistGuid = await authorizationService.LoginJournalist(loginJournalistBo);
        return Ok(journalistGuid);
    }

    [HttpPost("internal/authorization/logout")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        await authorizationService.Logout();
        return Ok();
    }

    [HttpGet("internal/authorization/me")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(CurrentUserBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CurrentUserBo>> GetCurrentUser()
    {
        var currentUser = await authorizationService.GetCurrentUser();
        
        if (currentUser == null)
            return Unauthorized();
            
        return Ok(currentUser);
    }
}
