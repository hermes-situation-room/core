#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record JournalistBo(Guid Uid,
    string? Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? ProfileIcon,
    string? ProfileIconColor,
    string Employer
)
    : UserBo(Uid,
        Password,
        PasswordHash,
        PasswordSalt,
        FirstName,
        LastName,
        EmailAddress,
        ProfileIcon,
        ProfileIconColor
    );
