namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public class TagController(IControllerInfrastructure infra, ITagService tagService)
    : SituationRoomControllerBase(infra)
{
    [HttpGet("internal/tags")]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_INTERNAL_TAG])]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public Task<ActionResult<IReadOnlyList<string>>> GetAllTags() =>
        Task.FromResult<ActionResult<IReadOnlyList<string>>>(Ok(tagService.GetAllTags()));
}
