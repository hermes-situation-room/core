namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Data.Interface;

public interface IActivistService
{
    Task<ActivistBo> GetActivistAsync(Guid activistUid);

    Task<IReadOnlyList<ActivistBo>> GetActivistsAsync();

    Task<Guid> CreateActivistAsync(ActivistBo activistBo);

    Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist);

    Task DeleteActivistAsync(Guid activistUid);
}

public class ActivistService(IActivistRepository activistRepository) : IActivistService
{
    public Task<ActivistBo> GetActivistAsync(Guid activistUid) => activistRepository.GetActivistBoAsync(activistUid);

    public Task<IReadOnlyList<ActivistBo>> GetActivistsAsync() => activistRepository.GetAllActivistBosAsync();

    public Task<Guid> CreateActivistAsync(ActivistBo activistBo) => activistRepository.AddAsync(activistBo);

    public Task<ActivistBo> UpdateActivistAsync(ActivistBo updatedActivist) =>
        activistRepository.UpdateAsync(updatedActivist);

    public Task DeleteActivistAsync(Guid activistUid) => activistRepository.DeleteAsync(activistUid);
}
