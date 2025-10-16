#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record ActivistBo(Guid Uid,
    string? Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? ProfileIcon,
    string? ProfileIconColor,
    string UserName,
    bool IsFirstNameVisible,
    bool IsLastNameVisible,
    bool IsEmailVisible
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
