#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record CreateJournalistRequestDto(string Password,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string Employer
);
