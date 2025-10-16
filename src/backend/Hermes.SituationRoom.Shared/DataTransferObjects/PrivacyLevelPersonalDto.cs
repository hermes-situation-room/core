#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record PrivacyLevelPersonalDto(Guid Uid,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible,
    Guid OwnerUid,
    Guid ConsumerUid
);
