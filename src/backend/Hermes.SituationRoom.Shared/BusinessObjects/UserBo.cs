#nullable enable
namespace Hermes.SituationRoom.Shared.BusinessObjects;

public abstract class UserBo
{
    public string Password { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? EmailAddress { get; set; }
}

public class ActivistBo : UserBo
{
    public string UserName { get; set; } = string.Empty;

    public bool IsFirstNameVisible { get; set; }

    public bool IsLastNameVisible { get; set; }

    public bool IsEmailVisible { get; set; }
}

public class JournalistBo : UserBo
{
    public string Employer { get; set; } = string.Empty;
}
