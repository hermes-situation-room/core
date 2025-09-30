namespace Hermes.SituationRoom.Api.Controllers.Base;

using AutoMapper;

public interface IControllerInfrastructure
{
    IMapper Mapper { get; set; }
}