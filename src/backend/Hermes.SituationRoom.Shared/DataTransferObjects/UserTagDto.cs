namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UserTagDto(
    Guid Uid,
    Guid OwnerUid,
    Guid TaggedUserUid,
    string TagName,
    string Notes,
    DateTime CreatedAt
);

public record CreateUserTagDto(
    Guid OwnerUid,
    Guid TaggedUserUid,
    string TagName,
    string Notes
);

public record UpdateUserTagDto(
    string TagName,
    string Notes
);

