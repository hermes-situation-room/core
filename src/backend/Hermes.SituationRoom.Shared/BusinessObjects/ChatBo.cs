#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ChatBo(Guid User1Uid,
    Guid User2Uid)
{
    public Guid? Uid { get; init; }
}