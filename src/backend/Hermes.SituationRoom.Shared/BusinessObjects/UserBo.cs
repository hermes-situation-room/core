#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserBo(Guid Uid,
    string? Password,
    byte[]? PasswordHash,
    byte[]? PasswordSalt,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? ProfileIcon,
    string? ProfileIconColor,
    string? Bio = null,
    string? PublishedWorkLink = null,
    string? LinkedInUrl = null,
    string? TwitterUrl = null,
    string? WebsiteUrl = null
);
