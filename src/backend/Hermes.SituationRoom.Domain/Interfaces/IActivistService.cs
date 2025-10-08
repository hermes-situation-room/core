namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;

public interface IActivistService
{
    Task<ActivistBo> GetActivistAsync(Guid activistUid);

    Task<IReadOnlyList<ActivistBo>> GetActivistsAsync();

    Task<Guid> CreateActivistAsync(ActivistBo activistBo);

    Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist);

    Task DeleteActivistAsync(Guid activistUid);
}
