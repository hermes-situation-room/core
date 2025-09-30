namespace Hermes.SituationRoom.Api.Controllers.Base;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

[Route("services/api/")]
[ApiController]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
public abstract class SituationRoomControllerBase(IControllerInfrastructure controllerInfrastructure) : ControllerBase
{
    protected IControllerInfrastructure Infrastructure { get; set; } = controllerInfrastructure;
}