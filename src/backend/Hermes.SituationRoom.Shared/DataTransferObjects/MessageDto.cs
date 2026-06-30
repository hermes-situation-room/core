#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record MessageDto(Guid Uid,
    string Content,
    Guid SenderUid,
    Guid ChatUid,
    DateTime Timestamp,
    DateTime? ExpiresAt = null,
    bool IsViewOnce = false,
    // True once a view-once message has been consumed for the requesting user
    // (its Content is blanked on the way out and must not be shown again).
    bool IsConsumed = false
);
