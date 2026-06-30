namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record TagRequestBo(
    Guid Uid,
    Guid RequestedByUid,
    string Name,
    string Status,
    Guid? ReviewedByUid,
    DateTime? ReviewedAt,
    DateTime CreatedAt
);
