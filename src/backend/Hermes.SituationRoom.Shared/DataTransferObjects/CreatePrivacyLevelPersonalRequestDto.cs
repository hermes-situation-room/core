#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreatePrivacyLevelPersonalRequestDto(bool? IsFirstNameVisible,
    bool? IsLastNameVisible,
    bool? IsEmailVisible,
    Guid OwnerUid,
    Guid ConsumerUid
);
