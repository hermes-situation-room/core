namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateChatRequestDto(Guid User1Uid,
    Guid User2Uid
);
