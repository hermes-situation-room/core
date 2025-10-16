namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateCommentRequestDto(Guid CreatorUid,
    Guid PostUid,
    string Content
);
