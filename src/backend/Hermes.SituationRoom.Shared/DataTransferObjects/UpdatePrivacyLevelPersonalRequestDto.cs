#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UpdatePrivacyLevelPersonalRequestDto(Guid Uid,
    bool? IsFirstNameVisible,
    bool? IsLastNameVisible,
    bool? IsEmailVisible
);
