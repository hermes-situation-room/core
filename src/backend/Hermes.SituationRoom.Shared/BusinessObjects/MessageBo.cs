#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record MessageBo(string Content,
    Guid SenderUid,
    Guid ChatUid,
    DateTime Timestamp)
{
    public Guid? Uid { get; init; }
}