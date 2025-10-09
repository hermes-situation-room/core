#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record NewMessageDto(string Content,
    Guid SenderUid,
    Guid ChatUid
);