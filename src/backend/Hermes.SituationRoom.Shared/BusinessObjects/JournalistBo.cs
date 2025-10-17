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
    string Employer,
    string? Bio = null,
    string? PublishedWorkLink = null,
    string? LinkedInUrl = null,
    string? TwitterUrl = null,
    string? WebsiteUrl = null,
    List<Guid>? SpecializationTagUids = null
)
    : UserBo(Uid,
        Password,
        PasswordHash,
        PasswordSalt,
        FirstName,
        LastName,
        EmailAddress,
        ProfileIcon,
        ProfileIconColor,
        Bio,
        PublishedWorkLink,
        LinkedInUrl,
        TwitterUrl,
        WebsiteUrl
    );
