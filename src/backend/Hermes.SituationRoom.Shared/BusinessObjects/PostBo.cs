namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record PostBo(Guid Uid,
    DateTime Timestamp,
    string Title,
    string Description,
    string Content,
    Guid CreatorUid,
    int PrivacyLevel
);
