namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;
using Shared.DataTransferObjects;

public interface IActivistService
{
    Task<ActivistBo> GetActivistAsync(Guid activistUid);

    Task<IReadOnlyList<ActivistBo>> GetActivistsAsync();

    Task<Guid> CreateActivistAsync(ActivistBo activistBo);

    Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist);

    Task<ActivistBo> UpdateActivistVisibilityAsync(Guid activistUid, UpdateActivistPrivacyLevelDto updateActivistPrivacyLevelDto);

    Task DeleteActivistAsync(Guid activistUid);
}
