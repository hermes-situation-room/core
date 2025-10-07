namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateCommentDto(Guid Uid,
    Guid CreatorUid,
    Guid PostUid,
    string Content
);
