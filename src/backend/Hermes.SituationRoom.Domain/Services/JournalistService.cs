namespace Hermes.SituationRoom.Domain.Services;

using Hermes.SituationRoom.Shared.BusinessObjects;
using Hermes.SituationRoom.Data.Interface;
using Interfaces;

public class JournalistService(IJournalistRepository journalistRepository) : IJournalistService
{
    public Task<JournalistBo> GetJournalistAsync(Guid journalistUid) =>
        journalistRepository.GetJournalistBoAsync(journalistUid);

    public Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync() => journalistRepository.GetAllJournalistBosAsync();

    public Task<Guid> CreateJournalistAsync(JournalistBo journalistBo) => journalistRepository.AddAsync(journalistBo);

    public Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist) =>
        journalistRepository.UpdateAsync(updatedJournalist);

    public Task DeleteJournalistAsync(Guid journalistUid) => journalistRepository.DeleteAsync(journalistUid);
}
