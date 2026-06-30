namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record TagRequestDto(
    Guid Uid,
    Guid RequestedByUid,
    string Name,
    string Status,
    Guid? ReviewedByUid,
    DateTime? ReviewedAt,
    DateTime CreatedAt
);

public record CreateTagRequestDto(
    string Name
);
