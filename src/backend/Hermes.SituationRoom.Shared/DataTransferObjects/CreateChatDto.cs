namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateChatDto(Guid User1Uid,
    Guid User2Uid)
{
    public Guid Uid { get; init; }
}
