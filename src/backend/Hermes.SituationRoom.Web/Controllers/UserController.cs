namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class UserController(IControllerInfrastructure infra, IUserService userService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserBo>> GetUser(Guid uid) => Ok(await userService.GetUserAsync(uid));

    [HttpGet("internal/user/profile/")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserBo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserBo>> GetUserProfile([FromQuery] Guid uid, [FromQuery] Guid? consumerUid) =>
        Ok(await userService.GetUserProfileAsync(uid, consumerUid ?? Guid.Empty));

    [HttpGet("internal/user/display-name/{uid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetDisplayName(Guid uid) => Ok(await userService.GetDisplayNameAsync(uid));

    [HttpGet("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(IReadOnlyList<UserBo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserBo>>> GetUsers() => Ok(await userService.GetUsersAsync());

    [HttpPost("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] UserBo userBo) =>
        Ok(await userService.CreateUserAsync(userBo));

    [HttpPut("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserBo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserBo>> UpdateUser([FromBody] UserBo userBo, Guid uid) =>
        Ok(await userService.UpdateUserAsync(userBo with { Uid = uid, }));

    [HttpDelete("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(Guid uid)
    {
        await userService.DeleteUserAsync(uid);
        return NoContent();
    }
}
