#nullable enable
namespace Hermes.SituationRoom.Shared.DataTransferObjects;

public record UserProfileDto(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? UserName,
    string? Employer
);
