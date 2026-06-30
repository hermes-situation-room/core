namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record PostLikeBo(
    Guid Uid,
    Guid PostUid,
    Guid UserUid,
    DateTime Timestamp
);

