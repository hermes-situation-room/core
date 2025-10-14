namespace Hermes.SituationRoom.Shared.BusinessObjects;

public class CurrentUserBo
{
    public Guid UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
}
