#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record JournalistDto(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string Employer
);
