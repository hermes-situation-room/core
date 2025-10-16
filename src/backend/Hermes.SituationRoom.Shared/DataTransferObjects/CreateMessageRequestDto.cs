#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateMessageRequestDto(string Content,
    Guid SenderUid,
    Guid ChatUid
);
