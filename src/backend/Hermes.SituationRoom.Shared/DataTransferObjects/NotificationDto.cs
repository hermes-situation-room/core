namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record NotificationDto(
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

public record CreateNotificationDto(
    Guid UserUid,
    string Title,
    string Message,
    string Type,
    Guid? RelatedEntityUid,
    string RelatedEntityType
);

