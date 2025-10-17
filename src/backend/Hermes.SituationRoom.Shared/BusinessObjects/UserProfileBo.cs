#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public record UserProfileBo(Guid Uid,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? UserName,
    string? Employer,
    string? ProfileIcon,
    string? ProfileIconColor,
    string? Bio = null,
    string? PublishedWorkLink = null,
    string? LinkedInUrl = null,
    string? TwitterUrl = null,
    string? WebsiteUrl = null,
    List<string>? Specializations = null
);
