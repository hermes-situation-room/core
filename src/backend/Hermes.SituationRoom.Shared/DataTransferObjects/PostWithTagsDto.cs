namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record PostWithTagsDto(Guid Uid,
    DateTime Timestamp,
    string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    IReadOnlyList<string> Tags
);
