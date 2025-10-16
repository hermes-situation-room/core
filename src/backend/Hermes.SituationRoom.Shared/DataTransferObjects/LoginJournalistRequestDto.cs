#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record LoginJournalistRequestDto(string Password,
    string EmailAddress
);
