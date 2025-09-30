namespace Hermes.SituationRoom.Api.Controllers.Base;

using AutoMapper;

public class ControllerInfrastructure
(
    IMapper mapper
)
    : IControllerInfrastructure
{
    public IMapper Mapper { get; set; } = mapper;
}