namespace Hermes.SituationRoom.Domain.Interfaces;

using Shared.BusinessObjects;

public interface IJournalistService
{
    Task<JournalistBo> GetJournalistAsync(Guid journalistUid);

    Task<IReadOnlyList<JournalistBo>> GetJournalistsAsync();

    Task<Guid> CreateJournalistAsync(JournalistBo journalistBo);

    Task<JournalistBo> UpdateJournalistAsync(JournalistBo updatedJournalist);

    Task DeleteJournalistAsync(Guid journalistUid);
}
