namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreatePostDto(Guid Uid,
    string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    IReadOnlyList<string> Tags
);
