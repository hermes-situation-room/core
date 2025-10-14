#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record JournalistBo(Guid Uid,
    string? Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string Employer
)
    : UserBo(Uid,
        Password,
        PasswordHash,
        PasswordSalt,
        FirstName,
        LastName,
        EmailAddress
    );
