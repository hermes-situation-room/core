#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ChatBo
{
    public Guid? Uid { get; set; }

    public Guid User1Uid { get; set; }

    public Guid User2Uid { get; set; }
}
