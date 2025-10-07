namespace Hermes.SituationRoom.Api.Controllers;

using Base;
using Configurations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

public class AboutController(IControllerInfrastructure controllerInfrastructure)
    : SituationRoomControllerBase(controllerInfrastructure)
{
    [HttpGet("external/about")]
    [SwaggerOperation(Tags = [SwaggerTagDescriptions.ENDPOINT_TAG_EXTERNAL])]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAsync()
    {
        return Ok("Hermes Situation Room Api up and running.");
    }
}
