namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserTagBo(
    Guid Uid,
    Guid OwnerUid,
    Guid TaggedUserUid,
    string TagName,
    string Notes,
    DateTime CreatedAt
);

