#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record MessageDto(Guid Uid,
    string Content,
    Guid SenderUid,
    Guid ChatUid,
    DateTime Timestamp
);
