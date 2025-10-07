namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.BusinessObjects;
using Swashbuckle.AspNetCore.Annotations;

public class UserController(IControllerInfrastructure infra, IUserService userService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<UserBo>> GetUser(Guid uid) => Ok(await userService.GetUserAsync(uid));

    [HttpGet("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<IReadOnlyList<UserBo>>> GetUsers() => Ok(await userService.GetUsersAsync());

    [HttpPost("internal/user/")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<Guid>> CreateUser([FromBody] UserBo userBo) =>
        Ok(await userService.CreateUserAsync(userBo));

    [HttpPut("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult<UserBo>> UpdateUser([FromBody] UserBo userBo) =>
        Ok(await userService.UpdateUserAsync(userBo));

    [HttpDelete("internal/user/{uid:guid}")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL])]
    public async Task<ActionResult> DeleteUser(Guid uid)
    {
        await userService.DeleteUserAsync(uid);
        return NoContent();
    }
}
