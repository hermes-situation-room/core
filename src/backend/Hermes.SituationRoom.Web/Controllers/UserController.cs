namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DataTransferObjects;
using Swashbuckle.AspNetCore.Annotations;

[Authorize]
public class UserController(IControllerInfrastructure infra, IUserService userService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUser(Guid uid) => Ok(await userService.GetUserAsync(uid));

    [HttpGet("internal/user/profile/")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile([FromQuery] Guid uid, [FromQuery] Guid consumerUid) =>
        Ok(await userService.GetUserProfileAsync(uid, consumerUid));

    [HttpGet("internal/user/display-name/{uid:guid}")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetDisplayName(Guid uid) => Ok(await userService.GetDisplayNameAsync(uid));

    [HttpGet("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers() => Ok(await userService.GetUsersAsync());
    
    [HttpGet("internal/user/get-id-by-email-or-username/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetUserIdByEmailOrUsernameAsync([FromQuery] string mailOrUsername) =>
        Ok(await userService.GetUserIdByEmailOrUsernameAsync(mailOrUsername));

    [HttpPost("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequestDto createUserDto) =>
        Ok(await userService.CreateUserAsync(createUserDto));

    [HttpPut("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_USER])]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserRequestDto updateUserDto, Guid uid) =>
        Ok(await userService.UpdateUserAsync(updateUserDto with { Uid = uid }));

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
