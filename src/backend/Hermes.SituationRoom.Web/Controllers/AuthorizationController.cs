namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;
using IAuthorizationService = Domain.Interfaces.IAuthorizationService;

public class AuthorizationController(IControllerInfrastructure infra, IAuthorizationService authorizationService)
    : SituationRoomControllerBase(infra)
{
    [HttpPost("internal/authorization/login/activist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> LoginActivist([FromBody] LoginActivistRequestDto loginActivistDto)
    {
        var activistGuid = await authorizationService.LoginActivist(loginActivistDto);
        return Ok(activistGuid);
    }

    [HttpPost("internal/authorization/login/journalist")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> LoginJournalist([FromBody] LoginJournalistRequestDto loginJournalistDto)
    {
        var journalistGuid = await authorizationService.LoginJournalist(loginJournalistDto);
        return Ok(journalistGuid);
    }

    [HttpPost("internal/authorization/logout")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await authorizationService.Logout();
        return Ok();
    }

    [HttpGet("internal/authorization/me")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_AUTHORIZATION])]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var currentUser = await authorizationService.GetCurrentUser();
        
        if (currentUser == null)
            return Unauthorized();
            
        return Ok(currentUser);
    }
}
