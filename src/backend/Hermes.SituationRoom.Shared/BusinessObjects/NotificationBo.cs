namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record NotificationBo(
    Guid Uid,
    Guid UserUid,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    Guid? RelatedEntityUid,
    string RelatedEntityType
);

