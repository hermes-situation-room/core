#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserProfileBo(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? UserName,
    string? Employer
);
