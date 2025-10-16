namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UpdatePostRequestDto(Guid Uid,
    string Title,
    string Description,
    string Content,
    IReadOnlyList<string> Tags
);
