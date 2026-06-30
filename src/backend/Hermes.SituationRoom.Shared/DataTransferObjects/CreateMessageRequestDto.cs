#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateMessageRequestDto(string Content,
    Guid SenderUid,
    Guid ChatUid,
    // Optional disappear timer in seconds; when set, ExpiresAt = now + this many seconds.
    int? DisappearAfterSeconds = null,
    bool IsViewOnce = false,
    // Optional ids of attachments (already uploaded by the sender) to link to the message.
    IReadOnlyList<Guid>? AttachmentIds = null
);
