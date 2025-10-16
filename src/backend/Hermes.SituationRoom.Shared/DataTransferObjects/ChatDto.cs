#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record ChatDto(Guid Uid,
    Guid User1Uid,
    Guid User2Uid
);
