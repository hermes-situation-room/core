namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.DataTransferObjects;

public interface IAuthorizationService
{
    Task<Guid> LoginActivist(LoginActivistRequestDto activistLoginDto);
    Task<Guid> LoginJournalist(LoginJournalistRequestDto journalistLoginDto);
    Task Logout();
    Task<CurrentUserDto?> GetCurrentUser();
}
