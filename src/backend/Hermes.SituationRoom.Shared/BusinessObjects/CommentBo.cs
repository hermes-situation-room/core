namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record CommentBo(Guid Uid,
    DateTime Timestamp,
    Guid CreatorUid,
    Guid PostUid,
    string Content
);
