namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CommentDto(Guid Uid,
    DateTime Timestamp,
    Guid CreatorUid,
    Guid PostUid,
    string Content
);
