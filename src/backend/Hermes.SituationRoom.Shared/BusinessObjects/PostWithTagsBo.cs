namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record PostWithTagsBo(Guid Uid,
    DateTime Timestamp,
    string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    IReadOnlyList<string> Tags
);
