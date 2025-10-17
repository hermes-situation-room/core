namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UpdateActivistPrivacyLevelRequestDto(bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible
);
