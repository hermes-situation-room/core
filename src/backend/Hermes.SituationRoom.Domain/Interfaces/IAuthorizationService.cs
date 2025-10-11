namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
public interface IAuthorizationService
{
    Task<Guid> LoginActivist(LoginActivistBo activistLoginBo);
    Task<Guid> LoginJournalist(LoginJournalistBo journalistLoginBo);
}
