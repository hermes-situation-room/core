namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreatePostRequestDto(string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    int PrivacyLevel,
    IReadOnlyList<string> Tags
);
