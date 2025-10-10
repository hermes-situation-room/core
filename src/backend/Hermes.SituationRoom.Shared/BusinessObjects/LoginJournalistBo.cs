#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record LoginJournalistBo(
    string Password,
    string EmailAddress
);